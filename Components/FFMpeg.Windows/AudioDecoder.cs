using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.FFMpeg;

/// <summary>
/// Decodes audio packets into audio buffers using FFmpeg.
/// Input must use a non-dropping DeliveryPolicy; dropping packets causes decoding errors.
/// </summary>
public sealed class AudioDecoder : IConsumer<Shared<Packet>>, IProducer<AudioBuffer>, INotifyPropertyChanged, IDisposable {

    /// <summary>
    /// Timestamps in files are rounded, e.g. to 1 ms in Matroska. Smaller differences are not treated as gaps.
    /// </summary>
    private static readonly TimeSpan ContinuityTolerance = TimeSpan.FromMilliseconds(10);

    private readonly FFMpegInterop.AudioDecoder _decoder = new();
    private readonly List<AudioSamples> _samples = new();

    #region Ports
    public Receiver<Shared<Packet>> In { get; }

    public Emitter<AudioBuffer> Out { get; }
    #endregion

    #region Options
    private bool discardRemainingOnStop;

    /// <summary>
    /// When enabled, audio remaining in the decoder is discarded when the input ends.
    /// </summary>
    public bool DiscardRemainingOnStop {
        get => discardRemainingOnStop;
        set => SetProperty(ref discardRemainingOnStop, value);
    }

    private ILogger? logger;

    public ILogger? Logger {
        get => logger;
        set => SetProperty(ref logger, value);
    }
    #endregion

    /// <summary>
    /// Originating time of the start of the file.
    /// </summary>
    private DateTime? fileStartTime;
    private DateTime lastOriginatingTime = DateTime.MinValue;
    private DateTime continuousStartTime;
    private long continuousSampleCount;
    private int continuousSampleRate;
    private WaveFormat? waveFormat;
    private SampleFormat sampleFormat = SampleFormat.None;

    public AudioDecoder(Pipeline pipeline) {
        In = pipeline.CreateReceiver<Shared<Packet>>(this, Process, nameof(In));
        Out = pipeline.CreateEmitter<AudioBuffer>(this, nameof(Out));

        In.Unsubscribed += OnUnsubscribed;
    }

    private void Process(Shared<Packet> packet, Envelope envelope) {
        // Envelope time of a packet is its decoding time
        fileStartTime ??= envelope.OriginatingTime - TimeSpan.FromTicks(packet.Resource.DecodingTimeOffset);

        _decoder.Decode(packet.Resource, _samples);
        PostSamples(finalOriginatingTime: null);
    }

    /// <summary>
    /// Flushes the decoder when the input ends.
    /// </summary>
    private void OnUnsubscribed(DateTime finalOriginatingTime) {
        if (DiscardRemainingOnStop) {
            return;
        }
        _decoder.Flush(_samples);
        PostSamples(finalOriginatingTime);
    }

    private void PostSamples(DateTime? finalOriginatingTime) {
        var late = 0;
        var unordered = 0;
        foreach (var samples in _samples) {
            // Counted in samples from the last gap, so that buffers are exactly adjacent
            var startTime = (DateTime)fileStartTime! + TimeSpan.FromTicks(samples.PresentationTimeOffset);
            var expectedStartTime = continuousStartTime + SamplesToTimeSpan(continuousSampleCount, continuousSampleRate);
            if (continuousSampleRate != samples.SampleRate || (startTime - expectedStartTime).Duration() > ContinuityTolerance) {
                continuousStartTime = startTime;
                continuousSampleCount = 0;
                continuousSampleRate = samples.SampleRate;
            }
            continuousSampleCount += samples.SampleCount;
            // \psi timestamps audio buffers at their end
            var originatingTime = continuousStartTime + SamplesToTimeSpan(continuousSampleCount, continuousSampleRate);
            if (originatingTime <= lastOriginatingTime) {
                unordered++;
                continue;
            }
            if (originatingTime > finalOriginatingTime) {
                // The pipeline silently drops messages after its final originating time.
                late++;
                continue;
            }
            lastOriginatingTime = originatingTime;
            Out.Post(new AudioBuffer(samples.Data, GetWaveFormat(samples)), originatingTime);
        }
        _samples.Clear();
        if (unordered > 0) {
            Logger?.LogWarning("{Count} audio buffers whose timestamps are not later than the previous buffer are discarded.", unordered);
        }
        if (late > 0) {
            Logger?.LogWarning("{Count} audio buffers timestamped after the final originating time of the pipeline are discarded.", late);
        }
    }

    private static TimeSpan SamplesToTimeSpan(long sampleCount, int sampleRate) {
        return sampleRate > 0 ? TimeSpan.FromTicks(sampleCount * TimeSpan.TicksPerSecond / sampleRate) : TimeSpan.Zero;
    }

    private WaveFormat GetWaveFormat(AudioSamples samples) {
        if (waveFormat is null || sampleFormat != samples.Format || waveFormat.SamplesPerSec != samples.SampleRate || waveFormat.Channels != samples.ChannelCount) {
            var rate = samples.SampleRate;
            var channels = samples.ChannelCount;
            waveFormat = samples.Format switch {
                SampleFormat.U8 => WaveFormat.CreatePcm(rate, 8, channels),
                SampleFormat.S16 => WaveFormat.CreatePcm(rate, 16, channels),
                SampleFormat.S32 => WaveFormat.CreatePcm(rate, 32, channels),
                SampleFormat.FLT => WaveFormat.CreateIeeeFloat(rate, channels),
                SampleFormat.DBL => WaveFormat.Create(WaveFormatTag.WAVE_FORMAT_IEEE_FLOAT, rate, 64, channels, channels * 8, rate * channels * 8),
                _ => throw new NotSupportedException($"Sample format {samples.Format} is not supported."),
            };
            sampleFormat = samples.Format;
        }
        return waveFormat;
    }

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        In.Unsubscribed -= OnUnsubscribed;
        _samples.Clear();
        _decoder.Dispose();
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
