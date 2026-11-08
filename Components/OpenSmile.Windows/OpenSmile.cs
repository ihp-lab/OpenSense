using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using OpenSense.Components.Audio;
using OpenSmileInterop;

namespace OpenSense.Components.OpenSmile;

/// <summary>
/// Runs an openSMILE configuration on audio streams.
/// Ports are the cExternalAudioSource and cExternalSink instances of the configuration, keyed by instance name.
/// All sources share one timeline: a source that starts later is padded with silence, and missing audio is replaced by silence,
/// so that features of all sources stay aligned with the originating times of their input.
/// </summary>
public sealed class OpenSmile : INotifyPropertyChanged, IDisposable {

    /// <summary>
    /// Deviation between the originating time of a buffer and the end of the audio written so far that is tolerated without padding or trimming.
    /// It should be as low as possible, while never triggering when no audio is dropped.
    /// </summary>
    private const int AlignmentToleranceSamples = 1;

    private const int SilenceChunkSeconds = 1;

    /// <summary>
    /// Size of the WAVEFORMATEXTENSIBLE fields after WAVEFORMATEX: valid bits per sample, channel mask and subtype.
    /// </summary>
    private const int ExtensibleExtraSize = 22;

    private static readonly Guid SubFormatPcm = new("00000001-0000-0010-8000-00aa00389b71");

    private static readonly Guid SubFormatIeeeFloat = new("00000003-0000-0010-8000-00aa00389b71");

    /// <summary>
    /// How much audio is held back while waiting for every connected source to start.
    /// The pipeline does not order first messages across inputs, so the timeline can only start once all sources have been seen.
    /// The limit keeps memory bounded when a connected source never delivers.
    /// </summary>
    private static readonly TimeSpan MaxStartWait = TimeSpan.FromSeconds(5);

    private readonly Pipeline _pipeline;
    private readonly Extractor _extractor;
    private readonly SourceState[] _sources;
    private readonly SinkState[] _sinks;
    private readonly List<Frame> _frames = new();
    private readonly List<PendingBuffer> _pendingBeforeStart = new();

    #region Ports
    public ImmutableDictionary<string, Receiver<AudioBuffer>> In { get; }

    public ImmutableDictionary<string, Emitter<FeatureFrame>> Out { get; }

    /// <summary>
    /// Feature names of each sink, posted once when the pipeline starts.
    /// </summary>
    public ImmutableDictionary<string, Emitter<string[]>> FeatureNamesOut { get; }
    #endregion

    #region Options
    private TimestampMode inputTimestampMode = TimestampMode.AtEnd; // \psi convention

    public TimestampMode InputTimestampMode {
        get => inputTimestampMode;
        set => SetProperty(ref inputTimestampMode, value);
    }

    private TimestampMode outputTimestampMode = TimestampMode.AtEnd; // \psi convention

    /// <summary>
    /// Whether a feature frame is posted with the originating time of the start or of the end of the audio it covers.
    /// </summary>
    public TimestampMode OutputTimestampMode {
        get => outputTimestampMode;
        set => SetProperty(ref outputTimestampMode, value);
    }

    private bool discardRemainingOnStop;

    /// <summary>
    /// When enabled, output that openSMILE produces only at the end of input (e.g. functionals over the whole input) is discarded on stop.
    /// </summary>
    public bool DiscardRemainingOnStop {
        get => discardRemainingOnStop;
        set => SetProperty(ref discardRemainingOnStop, value);
    }
    #endregion

    public ILogger? Logger { get; }

    public IReadOnlyList<AudioSourceInfo> Sources => _extractor.Sources;

    public IReadOnlyList<SinkInfo> Sinks => _extractor.Sinks;

    /// <summary>
    /// Originating time that corresponds to the first sample of every source.
    /// </summary>
    private DateTime? baseTime;

    private bool finished;

    private bool faulted;

    /// <param name="logger">Passed to the constructor instead of being set afterwards, so that messages emitted while parsing the configuration are not lost.</param>
    public OpenSmile(Pipeline pipeline, string configurationFilename, IEnumerable<KeyValuePair<string, string>>? options, int logLevel, ILogger? logger) {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Logger = logger;
        _extractor = new Extractor(configurationFilename, options, logLevel, OnLog);
        try {
            foreach (var info in _extractor.Sources) {
                ValidateSampleFormat(info);
            }
            _sources = _extractor.Sources.Select(info => new SourceState(info)).ToArray();
            _sinks = _extractor.Sinks.Select(info => new SinkState(info)).ToArray();

            var inputs = ImmutableDictionary.CreateBuilder<string, Receiver<AudioBuffer>>();
            for (var i = 0; i < _sources.Length; i++) {
                var sourceIndex = i;
                var name = _sources[i].Info.Name;
                var receiver = pipeline.CreateReceiver<AudioBuffer>(this, (buffer, envelope) => Process(sourceIndex, buffer, envelope), $"{nameof(In)}.{name}");
                receiver.Unsubscribed += finalOriginatingTime => OnUnsubscribed(receiver, finalOriginatingTime);
                _sources[i].Receiver = receiver;
                inputs.Add(name, receiver);
            }
            In = inputs.ToImmutable();

            var outputs = ImmutableDictionary.CreateBuilder<string, Emitter<FeatureFrame>>();
            var featureNamesOutputs = ImmutableDictionary.CreateBuilder<string, Emitter<string[]>>();
            foreach (var sink in _sinks) {
                var name = sink.Info.Name;
                sink.Emitter = pipeline.CreateEmitter<FeatureFrame>(this, $"{nameof(Out)}.{name}");
                sink.FeatureNamesEmitter = pipeline.CreateEmitter<string[]>(this, $"{nameof(FeatureNamesOut)}.{name}");
                outputs.Add(name, sink.Emitter);
                featureNamesOutputs.Add(name, sink.FeatureNamesEmitter);
            }
            Out = outputs.ToImmutable();
            FeatureNamesOut = featureNamesOutputs.ToImmutable();
        } catch {
            _extractor.Dispose();
            throw;
        }
        pipeline.PipelineRun += OnPipelineRun;
    }

    #region Pipeline Event Handlers
    private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {
        foreach (var sink in _sinks) {
            sink.FeatureNamesEmitter!.Post(sink.Info.ElementNames.ToArray(), e.StartOriginatingTime);
        }

        foreach (var source in _sources) {
            source.Connected = ((IReceiver)source.Receiver!).Source is not null;
        }
        var unconnected = _sources.Where(s => !s.Connected).Select(s => s.Info.Name).ToArray();
        if (unconnected.Length > 0 && unconnected.Length < _sources.Length) {
            Logger?.LogWarning("Sources {Sources} are not connected. Processing that depends on them does not progress until the end of input.", string.Join(", ", unconnected));
        }
    }
    #endregion

    #region Receivers
    private void Process(int sourceIndex, AudioBuffer buffer, Envelope envelope) {
        if (finished) {
            return;
        }
        try {
            Write(sourceIndex, buffer, envelope.OriginatingTime);
            PostFrames(finalOriginatingTime: null);
        } catch {
            faulted = true;
            throw;
        }
    }

    /// <summary>
    /// Flushes once every connected input has ended.
    /// Flushing here instead of in ISourceComponent.Stop is required: the pipeline closes upstream emitters only after all Stop calls have completed,
    /// so a Stop that waits for the final input never completes when the final originating time is later than the last message, e.g. with live capture.
    /// </summary>
    private void OnUnsubscribed(Receiver<AudioBuffer> receiver, DateTime finalOriginatingTime) {
        // The receiver being unsubscribed still has its source until its handlers return.
        var allEnded = In.Values.All(r => ReferenceEquals(r, receiver) || ((IReceiver)r).Source is null);
        if (!allEnded || finished) {
            return;
        }
        finished = true;
        if (DiscardRemainingOnStop || faulted) {
            return;
        }
        try {
            if (baseTime is null && _pendingBeforeStart.Count > 0) {
                StartTimeline();
            }
            _extractor.Finish(_frames);
            PostFrames(finalOriginatingTime);
        } catch {
            faulted = true;
            throw;
        }
    }
    #endregion

    #region Input
    private void Write(int sourceIndex, AudioBuffer buffer, DateTime originatingTime) {
        if (!buffer.HasValidData) {
            return;
        }
        var source = _sources[sourceIndex];
        ValidateFormat(source, buffer.Format);

        var start = InputTimestampMode switch {
            TimestampMode.AtEnd => originatingTime - buffer.Duration,
            TimestampMode.AtStart => originatingTime,
            _ => throw new InvalidOperationException($"{nameof(InputTimestampMode)} must be specified."),
        };
        if (baseTime is not null) {
            WriteAligned(sourceIndex, buffer, start);
            return;
        }

        // The message is only valid during the callback, so a held back buffer must be copied.
        _pendingBeforeStart.Add(new PendingBuffer(sourceIndex, buffer.DeepClone(), start));
        source.Started = true;
        var allStarted = _sources.All(s => s.Started || !s.Connected);
        var waited = _pendingBeforeStart.Max(p => p.Start + p.Buffer.Duration) - _pendingBeforeStart.Min(p => p.Start);
        if (allStarted || waited > MaxStartWait) {
            StartTimeline();
        }
    }

    /// <summary>
    /// Time zero is the earliest start among the held back buffers. Sources that start later are padded with silence by WriteAligned.
    /// </summary>
    private void StartTimeline() {
        baseTime = _pendingBeforeStart.Min(p => p.Start);
        var missing = _sources.Where(s => s.Connected && !s.Started).Select(s => s.Info.Name).ToArray();
        if (missing.Length > 0) {
            Logger?.LogWarning("Sources {Sources} have not delivered audio yet. Their audio earlier than {Time} will be discarded.", string.Join(", ", missing), baseTime.Value);
        }
        foreach (var pending in _pendingBeforeStart) {
            WriteAligned(pending.SourceIndex, pending.Buffer, pending.Start);
        }
        _pendingBeforeStart.Clear();
    }

    private void WriteAligned(int sourceIndex, AudioBuffer buffer, DateTime start) {
        var source = _sources[sourceIndex];
        var sampleRate = source.Info.SampleRate;
        var blockAlign = buffer.Format.BlockAlign;
        var expectedStart = baseTime!.Value + SamplesToTimeSpan(source.SamplesWritten, sampleRate);
        var deviationSamples = (long)Math.Round((start - expectedStart).TotalSeconds * sampleRate);
        var offset = 0;
        var count = buffer.Length;
        if (deviationSamples > AlignmentToleranceSamples) {
            var gap = SamplesToTimeSpan(deviationSamples, sampleRate);
            if (source.SamplesWritten == 0) {
                Logger?.LogInformation("Source {Source} starts {Gap} after the first source. It is padded with silence to align the sources.", source.Info.Name, gap);
            } else if (!source.GapWarned) {
                source.GapWarned = true;
                Logger?.LogWarning("Missing audio of {Gap} in source {Source} is replaced by silence, which affects the features. Use a non-dropping delivery policy for this input. Further gaps are logged as debug messages.", gap, source.Info.Name);
            } else {
                Logger?.LogDebug("Missing audio of {Gap} in source {Source} is replaced by silence.", gap, source.Info.Name);
            }
            WriteSilence(sourceIndex, deviationSamples);
        } else if (deviationSamples < -AlignmentToleranceSamples) {
            // Audio earlier than what has been written cannot be inserted anymore.
            var overlapSamples = Math.Min(-deviationSamples, count / blockAlign);
            offset = (int)overlapSamples * blockAlign;
            count -= offset;
            Logger?.LogWarning("Source {Source} overlaps already processed audio by {Overlap}. The overlapping audio is discarded.", source.Info.Name, SamplesToTimeSpan(-deviationSamples, sampleRate));
        }

        if (count > 0) {
            _extractor.Write(sourceIndex, buffer.Data, offset, count, _frames);
            source.SamplesWritten += count / blockAlign;
        }
    }

    private void WriteSilence(int sourceIndex, long samples) {
        var source = _sources[sourceIndex];
        var format = source.Format!;
        // Zero bytes are silence in every sample format that ValidateSampleFormat accepts.
        source.Silence ??= new byte[SilenceChunkSeconds * format.SamplesPerSec * format.BlockAlign];
        var remaining = samples * format.BlockAlign;
        while (remaining > 0) {
            var count = (int)Math.Min(remaining, source.Silence.Length);
            _extractor.Write(sourceIndex, source.Silence, 0, count, _frames);
            remaining -= count;
        }
        source.SamplesWritten += samples;
    }

    private static void ValidateFormat(SourceState source, WaveFormat format) {
        if (source.Format is not null) {
            if (!format.Equals(source.Format)) {
                throw new InvalidOperationException($"Audio format of source {source.Info.Name} changed from {Describe(source.Format)} to {Describe(format)}.");
            }
            return;
        }
        var info = source.Info;
        var matches = TryGetSampleType(format, out var isFloat)
            && isFloat == info.IsFloat
            && format.SamplesPerSec == info.SampleRate
            && format.Channels == info.Channels
            && format.BitsPerSample == info.BitsPerSample
            && format.BlockAlign == info.Channels * info.BytesPerSample;
        if (!matches) {
            // No silent conversion: resampling and mixing change the features, so they are left to explicit components.
            throw new InvalidOperationException($"Audio format of source {info.Name} does not match the openSMILE configuration. Expected {Describe(info)}, got {Describe(format)}. Convert the audio before this component, or change sampleRate, channels and nBits of the cExternalAudioSource.");
        }
        source.Format = format;
    }

    /// <summary>
    /// Accepts only the sample formats that openSMILE converts correctly: 16-bit, 24-bit in 3 bytes and 32-bit integer PCM, and 32-bit float.
    /// openSMILE reads 8-bit samples as signed, while 8-bit PCM is unsigned,
    /// and it reads 24-bit samples in 4 bytes from the low 3 bytes without sign extension, while they occupy the high 3 bytes.
    /// </summary>
    private static void ValidateSampleFormat(AudioSourceInfo info) {
        var supported = info.IsFloat
            ? info.BitsPerSample == 32 && info.BytesPerSample == 4
            : (info.BitsPerSample, info.BytesPerSample) is (16, 2) or (24, 3) or (32, 4);
        if (!supported) {
            throw new NotSupportedException($"Sample format of source {info.Name} is not supported: {Describe(info)}. openSMILE converts only 16-bit, 24-bit in 3 bytes (nBits = 24, nBPS = 3) and 32-bit integer samples, and 32-bit float samples (nBits = 33), correctly. Change nBits and nBPS of the cExternalAudioSource, and convert the audio before this component.");
        }
    }

    /// <summary>
    /// Whether the samples are integer PCM or IEEE float. WAVE_FORMAT_EXTENSIBLE is accepted with either subtype:
    /// its BitsPerSample is the container size, and the valid bits are aligned to the most significant end of the container,
    /// so the samples read correctly as integers of the container size.
    /// </summary>
    private static bool TryGetSampleType(WaveFormat format, out bool isFloat) {
        switch (format.FormatTag) {
            case WaveFormatTag.WAVE_FORMAT_PCM:
                isFloat = false;
                return true;
            case WaveFormatTag.WAVE_FORMAT_IEEE_FLOAT:
                isFloat = true;
                return true;
            case WaveFormatTag.WAVE_FORMAT_EXTENSIBLE when format is WaveFormatEx { ExtraInfo.Length: >= ExtensibleExtraSize } extensible:
                var subFormat = extensible.SubFormat;
                isFloat = subFormat == SubFormatIeeeFloat;
                return isFloat || subFormat == SubFormatPcm;
            default:
                isFloat = false;
                return false;
        }
    }
    #endregion

    #region Output
    private void PostFrames(DateTime? finalOriginatingTime) {
        if (_frames.Count == 0) {
            return;
        }
        if (baseTime is null) {
            // Frames produced without any input have no time reference.
            Logger?.LogWarning("{Count} frames produced without any input are discarded.", _frames.Count);
            _frames.Clear();
            return;
        }

        var dropped = 0;
        foreach (var frame in _frames) {
            var sink = _sinks[frame.SinkIndex];
            var emitter = sink.Emitter!;
            var startTime = baseTime.Value + SecondsToTimeSpan(frame.Time);
            var duration = SecondsToTimeSpan(frame.Length);
            var originatingTime = OutputTimestampMode switch {
                TimestampMode.AtStart => startTime,
                TimestampMode.AtEnd => startTime + duration,
                _ => throw new InvalidOperationException($"{nameof(OutputTimestampMode)} must be specified."),
            };
            var minimum = emitter.LastEnvelope.OriginatingTime + TimeSpan.FromTicks(1);
            if (originatingTime < minimum) {
                // Frame starts and ends are monotonic for fixed-step, turn-based and whole-input frames, but not for arbitrary frame lists.
                if (!sink.MonotonicityWarned) {
                    sink.MonotonicityWarned = true;
                    Logger?.LogWarning("Frames of sink {Sink} are not in increasing order of their timestamps. Their originating times are shifted to keep the stream monotonic, so they no longer match the covered audio exactly.", sink.Info.Name);
                }
                originatingTime = minimum;
            }
            if (originatingTime > finalOriginatingTime) {
                // The pipeline silently drops messages after its final originating time.
                dropped++;
                continue;
            }
            emitter.Post(new FeatureFrame(frame.Index, duration, frame.Values), originatingTime);
        }
        _frames.Clear();
        if (dropped > 0) {
            Logger?.LogWarning("{Count} frames timestamped after the final originating time of the pipeline are discarded.", dropped);
        }
    }
    #endregion

    #region Helpers
    private void OnLog(LogMessage message) {
        if (Logger is null) {
            return;
        }
        var level = message.Type switch {
            LogType.Error => LogLevel.Error,
            LogType.Warning => LogLevel.Warning,
            LogType.Debug => LogLevel.Debug,
            _ => LogLevel.Information,
        };
        if (string.IsNullOrEmpty(message.Module)) {
            Logger.Log(level, "{Text}", message.Text);
        } else {
            Logger.Log(level, "{Module}: {Text}", message.Module, message.Text);
        }
    }

    private static TimeSpan SamplesToTimeSpan(long samples, int sampleRate) => TimeSpan.FromTicks(samples * TimeSpan.TicksPerSecond / sampleRate);

    private static TimeSpan SecondsToTimeSpan(double seconds) => TimeSpan.FromTicks((long)Math.Round(seconds * TimeSpan.TicksPerSecond));

    private static string Describe(AudioSourceInfo info) => $"{(info.IsFloat ? "float" : "PCM")} {info.SampleRate} Hz, {info.Channels} channel(s), {info.BitsPerSample} bits in {info.BytesPerSample} bytes";

    private static string Describe(WaveFormat format) => $"{format.FormatTag} {format.SamplesPerSec} Hz, {format.Channels} channel(s), {format.BitsPerSample} bits, block align {format.BlockAlign}";
    #endregion

    #region Classes
    private sealed record class PendingBuffer(int SourceIndex, AudioBuffer Buffer, DateTime Start);

    private sealed class SourceState {

        public AudioSourceInfo Info { get; }

        public Receiver<AudioBuffer>? Receiver { get; set; }

        /// <summary>
        /// Whether an emitter was connected when the pipeline started. Only connected sources are waited for.
        /// </summary>
        public bool Connected { get; set; }

        public bool Started { get; set; }

        /// <summary>
        /// Format of the first buffer. Later buffers must match it.
        /// </summary>
        public WaveFormat? Format { get; set; }

        /// <summary>
        /// Samples written so far, including silence. Determines where the next buffer is expected on the shared timeline.
        /// </summary>
        public long SamplesWritten { get; set; }

        public bool GapWarned { get; set; }

        public byte[]? Silence { get; set; }

        public SourceState(AudioSourceInfo info) {
            Info = info;
        }
    }

    private sealed class SinkState {

        public SinkInfo Info { get; }

        public Emitter<FeatureFrame>? Emitter { get; set; }

        public Emitter<string[]>? FeatureNamesEmitter { get; set; }

        public bool MonotonicityWarned { get; set; }

        public SinkState(SinkInfo info) {
            Info = info;
        }
    }
    #endregion

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        _pipeline.PipelineRun -= OnPipelineRun;
        _extractor.Dispose();
    }
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (!EqualityComparer<T>.Default.Equals(field, value)) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}
