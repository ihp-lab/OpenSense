using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using Microsoft.Psi.Speech;
using OpenSense.Components.Audio;
using static System.Net.Mime.MediaTypeNames;

namespace OpenSense.Components.AzureSpeech {
    public sealed class AzureSpeechRecognizer : IConsumerProducer<(AudioBuffer, bool), IStreamingSpeechRecognitionResult>, INotifyPropertyChanged, IDisposable {

        private static readonly TimeSpan Tolerance = TimeSpan.FromMilliseconds(1);

        private static readonly TimeSpan FrameDuration = TimeSpan.FromSeconds(1d / 16_000);

        private static readonly TimeSpan BlankDuration = TimeSpan.FromSeconds(2);

        private static readonly byte[] Blank = new byte[(int)(BlankDuration.TotalSeconds * 16_000) * 1 * (16 / 8)];

        private static readonly long BlankTicks = BlankDuration.Ticks;

        private static readonly TimeSpan TimerInterval = TimeSpan.FromMilliseconds(100);

        private static readonly WaveFormat Format = WaveFormat.Create16kHz1Channel16BitPcm();

        private readonly PushAudioInputStream _stream;

        private readonly AudioConfig _config;

        private readonly SpeechRecognizer _recognizer;

        private readonly List<AudioSegment> _segments = new();

        private readonly List<AudioRequest> _requests = new();

        private readonly List<SpeechResponse> _responses = new();

        private readonly ReaderWriterLockSlim _requestLock = new ReaderWriterLockSlim();

        private readonly ReaderWriterLockSlim _responseLock = new ReaderWriterLockSlim();

        private readonly ProfanityOption _profanity;

        private readonly OutputFormat _mode;

        private readonly TimeSpan _durationThreshold;

        private readonly System.Timers.Timer _timer;

        private readonly TimeSpan _finalResultTimetout;

        private readonly TimestampMode _inputTimestampMode;

        private readonly TimestampMode _outputTimestampMode;

        private readonly string _joinSeparator;

        private readonly bool _outputAudio;

        private readonly bool _postEmptyResults;

        #region Ports
        public Receiver<(AudioBuffer, bool)> In { get; }

        public Emitter<IStreamingSpeechRecognitionResult> PartialOut { get; }

        public Emitter<IStreamingSpeechRecognitionResult> FinalOut { get; }

        public Emitter<IStreamingSpeechRecognitionResult> Out { get; }
        #endregion

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }

        private bool lastActivity;

        private AudioSegmentInfo? firstSegmentInfo;

        private AudioSegment? lastSegment;

        private long requestedLength;

        private double residualNumGapFrames;

        private Microsoft.CognitiveServices.Speech.SpeechRecognitionResult? partial;

        private DateTime activityStartTime;

        private long activityStartRequestedLength;

        public AzureSpeechRecognizer(Pipeline pipeline, AzureSpeechRecognizerConfiguration configuration) {
            _profanity = configuration.Profanity;
            _mode = configuration.Mode;
            _durationThreshold = configuration.DurationThreshold;
            _finalResultTimetout = configuration.ResultTimeout;
            _inputTimestampMode = configuration.InputTimestampMode;
            _outputTimestampMode = configuration.OutputTimestampMode;
            _joinSeparator = configuration.JoinSeparator;
            _outputAudio = configuration.OutputAudio;
            _postEmptyResults = configuration.PostEmptyResults;

            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, Process, nameof(In));
            PartialOut = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(PartialOut));
            FinalOut = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(FinalOut));
            Out = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;

            var speechConfig = SpeechConfig.FromSubscription(configuration.Key, configuration.Region);
            speechConfig.SpeechRecognitionLanguage = configuration.Language;
            speechConfig.OutputFormat = _mode;
            speechConfig.SetProfanity(_profanity);
            var format = AudioStreamFormat.GetWaveFormatPCM(16_000, 16, 1);
            _stream = AudioInputStream.CreatePushStream(format);
            _config = AudioConfig.FromStreamInput(_stream);
            _recognizer = new SpeechRecognizer(speechConfig, _config);
            _recognizer.SessionStarted += OnSessionStarted;
            _recognizer.Recognizing += OnRecognizing;
            _recognizer.Recognized += OnRecognized;
            _recognizer.Canceled += OnCanceled;
            _recognizer.SessionStopped += OnSessionStopped;
            var connection = Connection.FromRecognizer(_recognizer);
            connection.Connected += OnConnected;
            connection.Disconnected += OnDisconnected;

            _timer = new System.Timers.Timer(TimerInterval) {
                AutoReset = true,
            };
            _timer.Elapsed += OnElapsed;
        }

        private void Process((AudioBuffer, bool) data, Envelope envelope) {
            var (audio, activity) = data;
            if (audio.Format.FormatTag != Format.FormatTag || audio.Format.Channels != Format.Channels || audio.Format.BitsPerSample != Format.BitsPerSample || audio.Format.SamplesPerSec != Format.SamplesPerSec) {
                throw new NotSupportedException("Only 16kHz 16bit Mono PCM is supported.");
            }
            var activityChanged = activity != lastActivity;
            if (activityChanged) {
                lastActivity = activity;
                if (activity) {
                    activityStartTime = envelope.OriginatingTime;
                    activityStartRequestedLength = requestedLength;
                } else {
                    /* Send blank buffer to force evaluate. */
                    _stream.Write(Blank, Blank.Length);
                    requestedLength += Blank.Length;

                    /* Conclude as a request */
                    Debug.Assert(firstSegmentInfo is not null);
                    var firstInfo = (AudioSegmentInfo)firstSegmentInfo;
                    var length = (int)((requestedLength - Blank.Length) - activityStartRequestedLength);
                    var info = new AudioSegmentInfo(length, firstInfo.StartTime);
                    var offset = activityStartRequestedLength;
                    var adjustment = _inputTimestampMode switch { 
                        TimestampMode.AtStart => TimeSpan.Zero,
                        TimestampMode.AtEnd => audio.Duration,
                        _ => throw new InvalidOperationException(),
                    };
                    var audioStartTime = envelope.OriginatingTime - adjustment;
                    var trueEndTime = audioStartTime;
                    byte[]? buffer = null;
                    if (_outputAudio) {
                        Debug.Assert(_segments.Count > 0);
                        var segmentNullable = Combine(_segments);
                        if (segmentNullable is { } segment) {
                            buffer = segment.Buffer;
                        }
                    }
                    var request = new AudioRequest(offset, buffer, length, info.StartTime, info.Duration, info.EstimatedEndTime, trueEndTime, BlankTicks);
                    _requestLock.EnterWriteLock();
                    try {
                        _requests.Add(request);
                    } finally {
                        _requestLock.ExitWriteLock();
                    }
                    Post();

                    /* Clean saved copy */
                    firstSegmentInfo = null;
                    residualNumGapFrames = 0;

                    Debug.Assert(lastSegment is not null);
                    var last = (AudioSegment)lastSegment;
                    if (_outputAudio) {
                        Debug.Assert(_segments.Count > 0);
                        Debug.Assert(_segments.Select(s => s.Buffer).Any(b => ReferenceEquals(b, last.Buffer)));
                        foreach (var s in _segments) {
                            ArrayPool<byte>.Shared.Return(s.Buffer);
                        }
                        _segments.Clear();
                    } else {
                        Debug.Assert(_segments.Count == 0);
                        ArrayPool<byte>.Shared.Return(last.Buffer);
                    }
                    lastSegment = null;
                }
            }
            if (activity) {
                /* Allocate */
                var adjustment = _inputTimestampMode switch {
                    TimestampMode.AtStart => TimeSpan.Zero,
                    TimestampMode.AtEnd => audio.Duration,
                    _ => throw new InvalidOperationException(),
                };
                var audioStartTime = envelope.OriginatingTime - adjustment;
                var buffer = ArrayPool<byte>.Shared.Rent(audio.Length);
                audio.Data.AsSpan(0, audio.Length).CopyTo(buffer);
                var segment = new AudioSegment(buffer, audio.Length, audioStartTime);

                /* Send paddings */
                if (lastSegment is { } last) {
                    var numPadFrames = NumPadFrames(last, segment, ref residualNumGapFrames);
                    if (numPadFrames > 0) {
                        var padLength = numPadFrames * 1 * (16 / 8);
                        var padBuffer = ArrayPool<byte>.Shared.Rent(padLength);
                        try {
                            padBuffer.AsSpan(0, padLength).Fill(0);
                            _stream.Write(padBuffer, padLength);
                            requestedLength += padLength;
                        } finally {
                            ArrayPool<byte>.Shared.Return(padBuffer);
                        }
                    }
                }

                /* Send segment */
                _stream.Write(segment.Buffer, segment.Length);
                requestedLength += segment.Length;

                /* Save for potential future use */
                if (activityChanged) {
                    Debug.Assert(firstSegmentInfo is null);
                    Debug.Assert(lastSegment is null);
                    firstSegmentInfo = segment.Info;
                }
                if (_outputAudio) {
                    _segments.Add(segment);
                } else {
                    if (lastSegment is { } last2) {
                        ArrayPool<byte>.Shared.Return(last2.Buffer);
                    }
                }
                lastSegment = segment;
            }
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            activityStartTime = args.StartOriginatingTime;
            _recognizer.StartContinuousRecognitionAsync().Wait();
            _timer.Start();
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            _timer.Stop();
            _recognizer.StopContinuousRecognitionAsync().Wait();
            //Discard unprocessed data
        }
        #endregion

        #region Speech Recognizer Handlers
        /// <remarks>Session started does not mean the connection is established.</remarks>
        private void OnSessionStarted(object? sender, SessionEventArgs args) {

        }

        private void OnRecognizing(object? sender, SpeechRecognitionEventArgs args) {
            Debug.Assert(args.Result.Reason == ResultReason.RecognizingSpeech);
            Debug.Assert(args.Result.Text is not null);
            Debug.Assert(!args.Result.Best().Any());
            _responseLock.EnterWriteLock();
            try {
                partial = args.Result;
            } finally {
                _responseLock.ExitWriteLock();
            }
            Post();
        }

        private void OnRecognized(object? sender, SpeechRecognitionEventArgs args) {
            switch (args.Result.Reason) {
                case ResultReason.RecognizedSpeech:
                    Debug.Assert(args.Result.Text is not null);
                    SpeechResponse response;
                    switch (_mode) {
                        case OutputFormat.Simple:
                            response = new SpeechResponse(
                                args.Result.OffsetInTicks,
                                args.Result.Duration,
                                candidates: [new SpeechResponseCandidate(args.Result.Text, Confidence: null), ]
                            );
                            break;
                        case OutputFormat.Detailed:
                            var candidates = args.Result
                                .Best()
                                .Select(c => new SpeechResponseCandidate(c.Text, c.Confidence))
                                .ToImmutableArray();
                            response = new SpeechResponse(
                                args.Result.OffsetInTicks,
                                args.Result.Duration,
                                candidates
                            );
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    _responseLock.EnterWriteLock();
                    try {
                        _responses.Add(response);
                    } finally {
                        _responseLock.ExitWriteLock();
                    }
                    Post();
                    break;
                case ResultReason.NoMatch:

                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private void OnCanceled(object? sender, SpeechRecognitionCanceledEventArgs args) {

        }

        private void OnSessionStopped(object? sender, SessionEventArgs args) {

        }
        #endregion

        #region Connection Handlers
        private void OnConnected(object? sender, ConnectionEventArgs args) {

        }

        private void OnDisconnected(object? sender, ConnectionEventArgs args) {

        }
        #endregion

        #region Timer Event Handlers
        private void OnElapsed(object? sender, ElapsedEventArgs args) {
            Post();
        }
        #endregion

        private void Post() {
            _requestLock.EnterUpgradeableReadLock();
            _responseLock.EnterUpgradeableReadLock();
            try {
                /* Step 1: Post as Final */
                var now = DateTime.UtcNow;
                var processed = 0;
                {
                    var span = CollectionsMarshal.AsSpan(_requests);
                    for (var i = 0; i < span.Length; i++) {
                        ref var request = ref span[i];
                        var startTicks = request.StreamStartTicks;//Use in lambda
                        var endTicks = request.StreamEndTicks2;//Use in lambda

                        var useless = _responses.TakeWhile(r => r.OffsetInTicks + r.Duration.Ticks < startTicks).Count();//Note: do not remove at now to reduce array copy //Note: some responses may have start time a little bit earlier then request start time
                        var responses = _responses.Skip(useless).TakeWhile(r => r.OffsetInTicks <= endTicks);
                        var count = responses.Count();
                        var covered = useless + count;
                        var hasSuccessor = _responses.Skip(covered).Any();
                        Debug.Assert(count == 0 || _responses.Skip(useless).Take(count).All(r => r.OffsetInTicks + r.Duration.Ticks >= startTicks));
                        //Debug.Assert(count == 0 || _responses.Skip(useless).Take(count).All(r => r.OffsetInTicks + r.Duration.Ticks <= endTicks));

                        /* Check wait conditions */
                        if (!hasSuccessor) {//no successive responses for other requests
                            if (request.Duration > TimeSpan.Zero) {//May have recognized results
                                var endTick = request.StreamStartTicks;
                                if (count > 0) { //Has any final result available to inference
                                    var last = responses.Last();
                                    endTick = last.OffsetInTicks + last.Duration.Ticks;
                                }
                                if (endTick < request.StreamEndTicks1) {//last result end time is still smaller than request end time
                                    var unrecognizedDuration = new TimeSpan(request.StreamEndTicks1 - endTick);
                                    if (unrecognizedDuration >= _durationThreshold) {//still a lot of unrecognized audio
                                        /* Check timeout */
                                        if (now - request.RequestTime <= _finalResultTimetout) {//not yet timeout
                                            break;//wait more final results
                                        } else {
                                            Logger?.LogDebug("Final result wait timeout. {count} final result(s) were received.", count);
                                        }
                                    } else {
                                        Logger?.LogDebug("The unrecognized audio {duration:F3} is shorter than threshold {threshold:F3}. Posting.", unrecognizedDuration.TotalSeconds, _durationThreshold.TotalSeconds);
                                    }
                                }
                            }
                        }

                        /* Post */
                        if (count > 0) {
                            var candidates = Combine(_responses.Skip(useless).Take(count));
                            var best = candidates.First();
                            var alternates = candidates
                                .Skip(1)
                                .Select(c => new SpeechRecognitionAlternate(c.Text, c.Confidence))
                                .ToImmutableArray();
                            if (_postEmptyResults || !string.IsNullOrEmpty(best.Text)) {
                                var audio = CopyAudio(request);
                                var result = new StreamingSpeechRecognitionResult(
                                    isFinal: true,
                                    text: best.Text,
                                    confidence: best.Confidence,
                                    alternates: alternates,
                                    audio: audio,
                                    duration: request.Duration
                                );
                                var timestamp = _outputTimestampMode switch {
                                    TimestampMode.AtStart => request.StartTime,
                                    TimestampMode.AtEnd => request.EndTime,
                                    _ => throw new InvalidOperationException(),
                                };
                                SafePost(result, timestamp, FinalOut);
                                SafePost(result, timestamp, Out);
                            }
                        }

                        /* Remove responses */
                        if (covered > 0) {
                            if (useless > 0) {
                                if (Logger is not null) {
                                    var text = Combine(_responses.Take(useless)).First().Text;
                                    if (string.IsNullOrEmpty(text)) {
                                        Logger.LogDebug("Discard {num} over-due final speech recognition result(s). These results are empty.", useless);
                                    } else {
                                        Logger.LogWarning("Discard {num} over-due final speech recognition result(s). Results: {texts}", useless, text);
                                    }
                                }
                            }

                            var clearPartial = partial is not null && partial.OffsetInTicks <= request.StreamEndTicks2;
                            _responseLock.EnterWriteLock();
                            try {
                                _responses.RemoveRange(0, covered);
                                if (clearPartial) {
                                    partial = null;
                                }
                            } finally {
                                _responseLock.ExitWriteLock();
                            }
                        }

                        processed++;
                    }
                }
                /* Remove processed requests, finals were posted */
                if (processed > 0) {
                    _requestLock.EnterWriteLock();
                    try {
                        {
                            var span = CollectionsMarshal.AsSpan(_requests);
                            for (var i = 0; i < processed; i++) {
                                ref var request = ref span[i];
                                if (request.Buffer is not null) {
                                    ArrayPool<byte>.Shared.Return(request.Buffer);
                                }
                            }
                        }
                        _requests.RemoveRange(0, processed);
                    } finally {
                        _requestLock.ExitWriteLock();
                    }
                }

                /* Step 2: Post as Partial */
                {
                    var startTicks = activityStartRequestedLength / 1 / (16 / 8) * FrameDuration.Ticks;
                    var endTicks = long.MaxValue;
                    var startTime = activityStartTime;

                    var pendingRequest = (AudioRequest?)null;
                    if (_requests.Count > 0) {
                        var r = _requests.First();
                        pendingRequest = r;

                        /* Set boundaries */
                        startTicks = r.StreamStartTicks;
                        endTicks = r.StreamEndTicks2;

                        /* Update start time to accurate value */
                        startTime = r.StartTime;
                    }

                    var candidates = ImmutableArray<SpeechResponseCandidate>.Empty;
                    var duration = (TimeSpan?)null;
                    var finalOnly = true;
                    var finalFlag = (bool?)null;

                    /* Combine finals */
                    var useless = _responses.TakeWhile(r => r.OffsetInTicks < startTicks).Count();
                    var responses = _responses.Skip(useless).TakeWhile(r => r.OffsetInTicks <= endTicks);
                    var count = responses.Count();
                    var covered = useless + count;
                    if (count > 0) {
                        duration = new TimeSpan(responses.Last().OffsetInTicks - startTicks) + responses.Last().Duration;

                        /* Combine */
                        candidates = Combine(_responses.Skip(useless).Take(count));

                        /* Record Flag */
                        var span = CollectionsMarshal.AsSpan(_responses);
                        ref var lastFinal = ref span[covered - 1];
                        finalFlag = lastFinal.PartialPostedFlag;
                        /* Mark Flag */
                        lastFinal.PartialPostedFlag = true;
                    }

                    /* Combine partial */
                    if (partial is not null) {
                        if (startTicks <= partial.OffsetInTicks && partial.OffsetInTicks + partial.Duration.Ticks <= endTicks) {
                            if (candidates.Length == 0) {
                                candidates = [new SpeechResponseCandidate(partial.Text, Confidence: null), ];
                            } else {
                                var temp = ImmutableArray<SpeechResponseCandidate>.Empty
                                    .Add(new SpeechResponseCandidate(partial.Text, Confidence: null));
                                candidates = Combine(candidates, temp).ToImmutableArray();
                            }
                            duration = new TimeSpan(partial.OffsetInTicks - startTicks) + partial.Duration;
                            finalOnly = false;
                        }
                    }
                    /* Post as partial */
                    var alreadyPosted = finalOnly && finalFlag == true;
                    if (!alreadyPosted && candidates.Length > 0) {
                        var best = candidates.First();
                        if (_postEmptyResults || !string.IsNullOrWhiteSpace(best.Text)) {
                            Debug.Assert(duration.HasValue);
                            var alternates = candidates
                                .Skip(1)
                                .Select(c => new SpeechRecognitionAlternate(c.Text, c.Confidence))
                                .ToImmutableArray();
                            var audio = pendingRequest is { } r ? CopyAudio(r) : null;
                            var result = new StreamingSpeechRecognitionResult(
                                isFinal: false,
                                text: best.Text,
                                confidence: best.Confidence,
                                alternates: alternates,
                                audio: null,//TODO: Add an option, copy data from reqeust.Buffer
                                duration: duration
                            );
                            var timestamp = _outputTimestampMode switch {
                                TimestampMode.AtStart => startTime,
                                TimestampMode.AtEnd => startTime + duration.Value,
                                _ => throw new InvalidOperationException(),
                            };
                            SafePost(result, timestamp, PartialOut);
                            SafePost(result, timestamp, Out);
                        }
                    }
                }
                if (partial is not null) {
                    _responseLock.EnterWriteLock();
                    try {
                        partial = null;
                    } finally {
                        _responseLock.ExitWriteLock();
                    }
                }

            } finally {
                _responseLock.ExitUpgradeableReadLock();
                _requestLock.ExitUpgradeableReadLock();
            }
        }

        private static void SafePost(IStreamingSpeechRecognitionResult result, DateTime originatingTime, Emitter<IStreamingSpeechRecognitionResult> emitter) {
            var safeTime = emitter.LastEnvelope.OriginatingTime + Tolerance;
            if (originatingTime < safeTime) {
                originatingTime = safeTime;
            }
            emitter.Post(result, originatingTime);
        }

        #region Helpers

        private static int NumPadFrames(in AudioSegment first, in AudioSegment second, ref double residualNumGapFrames) {
            var lastEndTime = first.EstimatedEndTime;
            var currentStartTime = second.StartTime;
            var gapDuration = currentStartTime - lastEndTime;
            var numGapFrames = residualNumGapFrames + gapDuration / FrameDuration;
            var numPadFrames = Math.Max(0, (int)numGapFrames);
            residualNumGapFrames = numGapFrames - numPadFrames;
            var numPadLength = numPadFrames * 1 * (16 / 8);
            return numPadLength;
        }

        private static AudioSegment? Combine(IReadOnlyList<AudioSegment> segments) {
            if (segments.Count == 0) {
                return null;
            }
            var first = segments.First();
            var length = first.Length;
            var residualNumGapFrames = 0d;
            using var offsetBuffer = MemoryPool<int>.Shared.Rent(segments.Count);
            var offsets = offsetBuffer.Memory.Span;
            offsets[0] = 0;
            using var padBuffer = MemoryPool<int>.Shared.Rent(segments.Count);
            var pads = padBuffer.Memory.Span;
            pads[0] = 0;
            for (var i = 1; i < segments.Count; i++) {
                var numPadFrames = NumPadFrames(segments[i - 1], segments[i], ref residualNumGapFrames);
                var numPadLength = numPadFrames * 1 * (16 / 8);
                length += numPadLength;
                pads[i] = numPadLength;
                offsets[i] = length;
                length += segments[i].Length;
            }
            if (length == 0) {
                return null;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            for (var i = 0; i < segments.Count; i++) {
                var segment = segments[i];
                var offset = offsets[i];
                var pad = pads[i];
                buffer.AsSpan(offset - pad, pad).Fill(0);
                segment.Buffer.AsSpan(0, segment.Length).CopyTo(buffer.AsSpan(offset));
            }
            var result = new AudioSegment(buffer, length, first.StartTime);
            return result;
        }

        private IEnumerable<SpeechResponseCandidate> Combine(IEnumerable<SpeechResponseCandidate> left, IEnumerable<SpeechResponseCandidate> right) {
            foreach (var l in left) {
                foreach (var r in right) {
                    if (string.IsNullOrEmpty(l.Text)) {
                        yield return r;
                    } else if (string.IsNullOrEmpty(r.Text)) {
                        yield return l;
                    } else {
                        yield return new SpeechResponseCandidate(l.Text + _joinSeparator + r.Text, l.Confidence * r.Confidence);
                    }
                }
            }
        }

        private ImmutableArray<SpeechResponseCandidate> Combine(IEnumerable<SpeechResponse> responses) {
            var result = responses
                .Select(r => r.Candidates)
                .Aggregate(Combine)
                .GroupBy(r => r.Text)
                .Select(g => new SpeechResponseCandidate(g.Key, g.Max(r => r.Confidence)))
                .OrderByDescending(r => r.Confidence)
                .ThenBy(r => r.Text.Length)
                .ThenBy(r => r.Text)
                .ToImmutableArray()
                ;
            return result;
        }

        private static AudioBuffer? CopyAudio(AudioRequest request) {
            if (request.Buffer is null) {
                return null;
            }
            byte[] buffer;
            if (request.Length == 0) {
                buffer = Array.Empty<byte>();
            } else {
                buffer = new byte[request.Length];
                request.Buffer.AsSpan(0, request.Length).CopyTo(buffer);
            }
            var result = new AudioBuffer(buffer, Format);
            return result;
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

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            foreach (var segment in _segments) {
                ArrayPool<byte>.Shared.Return(segment.Buffer);
            }

            _timer.Dispose();

            _recognizer.Dispose();
            _config.Dispose();
            _stream.Dispose();
        }
        #endregion

        #region Classes
        private readonly struct AudioSegmentInfo {

            /// <summary>
            /// Data length.
            /// </summary>
            public int Length { get; }

            public DateTime StartTime { get; }

            public DateTime EstimatedEndTime { get; }

            public TimeSpan Duration { get; }

            public AudioSegmentInfo(int length, DateTime startTime) {
                Length = length;
                StartTime = startTime;
                Duration = TimeSpan.FromSeconds((double)(length / 1 / (16 / 8)) / 16_000);
                EstimatedEndTime = startTime + Duration;
            }
        }

        private readonly struct AudioSegment {

            public AudioSegmentInfo Info { get; }

            /// <summary>
            /// Data length. Can be smaller than buffer length.
            /// </summary>
            public int Length => Info.Length;

            public DateTime StartTime => Info.StartTime;

            public DateTime EstimatedEndTime => Info.EstimatedEndTime;

            public TimeSpan Duration => Info.Duration;

            public byte[] Buffer { get; }

            public AudioSegment(byte[] buffer, int length, DateTime startTime) {
                Buffer = buffer;
                Info = new AudioSegmentInfo(length, startTime);
            }
        }

        private readonly struct AudioRequest {

            public byte[]? Buffer { get; }

            /// <summary>
            /// Data length. Can be smaller than buffer length.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// Reuqest start timestamp that can be used in \psi time scale.
            /// </summary>
            public DateTime StartTime { get; }

            /// <summary>
            /// Request end timestamp that can be used in \psi time scale.
            /// </summary>
            public DateTime EndTime { get; }

            /// <summary>
            /// The duration of the audio data.
            /// </summary>
            public TimeSpan Duration { get; }

            /// <summary>
            /// Start tick that is used to compare with response tick. It does not represent the actual start time.
            /// </summary>
            public long StreamStartTicks { get; }

            /// <summary>
            /// End tick that is used to compare with response tick. It does not represent the actual end time. It does not include ending blank.
            /// </summary>
            public long StreamEndTicks1 { get; }

            /// <summary>
            /// End tick that is used to compare with response tick. It does not represent the actual end time. It includes ending blank.
            /// </summary>
            public long StreamEndTicks2 { get; }

            /// <summary>
            /// Time that this request is formed. It is used to check request timeout.
            /// </summary>
            public DateTime RequestTime { get; }

            public AudioRequest(long streamOffset, byte[]? buffer, int length, DateTime startTime, TimeSpan duration, DateTime estimatedEndTime, DateTime trueEndTime, long endingBlankTicks) {
                Buffer = buffer;
                Length = length;
                StartTime = startTime;
                Duration = duration;
                var streamStartFrame = streamOffset / 1 / (16 / 8);
                StreamStartTicks = streamStartFrame * FrameDuration.Ticks;
                StreamEndTicks1 = StreamStartTicks + duration.Ticks;
                StreamEndTicks2 = StreamEndTicks1 + endingBlankTicks;
                RequestTime = DateTime.UtcNow;
                EndTime = trueEndTime < estimatedEndTime ? trueEndTime : estimatedEndTime;
            }
        }

        private struct SpeechResponse {

            public long OffsetInTicks { get; }

            public TimeSpan Duration { get; }

            public IEnumerable<SpeechResponseCandidate> Candidates { get; }

            /// <summary>
            /// A mutable flag to prevent outputing the same partial result.
            /// </summary>
            public bool PartialPostedFlag { get; set; }

            public SpeechResponse(long offsetInTicks, TimeSpan duration, IEnumerable<SpeechResponseCandidate> candidates) {
                OffsetInTicks = offsetInTicks;
                Duration = duration;
                Candidates = candidates;
            }
        }

        private readonly record struct SpeechResponseCandidate(string Text, double? Confidence);
        #endregion
    }
}
