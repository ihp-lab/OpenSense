using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using Microsoft.Psi.Speech;
using OpenSense.Components.Audio;
using Whisper.net;
using Whisper.net.Ggml;

namespace OpenSense.Components.Whisper.NET {

    public sealed class WhisperSpeechRecognizer : IConsumerProducer<(AudioBuffer, bool), IStreamingSpeechRecognitionResult>, IDisposable, INotifyPropertyChanged {

        /// <remarks>
        /// This value should be set as low as possible, while still being high enough to ensure that gap filling is never triggered when the delivery policy is set to Unlimited.
        /// </remarks>
        private const int GapSampleThreshold = 1;

        private readonly List<Section> _sections = new();

        private readonly List<SegmentData> _segments = new();

        private readonly Lazy<WhisperProcessor> _processor;

        #region Options
        private string modelDirectory = "";

        public string ModelDirectory {
            get => modelDirectory;
            set => SetProperty(ref modelDirectory, value);
        }

        private GgmlType modelType = GgmlType.BaseEn;

        public GgmlType ModelType {
            get => modelType;
            set => SetProperty(ref modelType, value);
        }

        private QuantizationType quantizationType = QuantizationType.Q5_1;

        public QuantizationType QuantizationType {
            get => quantizationType;
            set => SetProperty(ref quantizationType, value);
        }

        private bool forceDownload = false;

        public bool ForceDownload {
            get => forceDownload;
            set => SetProperty(ref forceDownload, value);
        }

        private TimeSpan downloadTimeout = TimeSpan.FromSeconds(15);

        public TimeSpan DownloadTimeout {
            get => downloadTimeout;
            set => SetProperty(ref downloadTimeout, value);
        }

        private bool lazyInitialization = false;

        public bool LazyInitialization {
            get => lazyInitialization;
            set => SetProperty(ref lazyInitialization, value);
        }

        private Language language = Language.English;

        public Language Language {
            get => language;
            set => SetProperty(ref language, value);
        }

        private string prompt = "";

        public string Prompt {
            get => prompt;
            set => SetProperty(ref prompt, value);
        }

        private SegmentationRestriction segmentationRestriction = SegmentationRestriction.OnePerUtterence;

        public SegmentationRestriction SegmentationRestriction {
            get => segmentationRestriction;
            set => SetProperty(ref segmentationRestriction, value);
        }

        private TimestampMode inputTimestampMode = TimestampMode.AtEnd;//\psi convention

        public TimestampMode InputTimestampMode {
            get => inputTimestampMode;
            set => SetProperty(ref inputTimestampMode, value);
        }

        private TimestampMode outputTimestampMode = TimestampMode.AtEnd;

        public TimestampMode OutputTimestampMode {
            get => outputTimestampMode;
            set => SetProperty(ref outputTimestampMode, value);
        }

        private bool outputPartialResults = false;

        public bool OutputPartialResults {
            get => outputPartialResults;
            set => SetProperty(ref outputPartialResults, value);
        }

        private TimeSpan partialEvalueationInverval = TimeSpan.FromMilliseconds(500);

        public TimeSpan PartialEvalueationInverval {
            get => partialEvalueationInverval;
            set => SetProperty(ref partialEvalueationInverval, value);
        }

        private bool outputAudio = false;

        public bool OutputAudio {
            get => outputAudio;
            set => SetProperty(ref outputAudio, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private double progress = 0;

        public double Progress {
            get => progress;
            private set => SetProperty(ref progress, value);
        }

        #region Ports
        public Receiver<(AudioBuffer, bool)> In { get; }

        public Emitter<IStreamingSpeechRecognitionResult> PartialOut { get; }

        public Emitter<IStreamingSpeechRecognitionResult> FinalOut { get; }

        public Emitter<IStreamingSpeechRecognitionResult> Out { get; } 
        #endregion

        private string? modelFilename;

        private TimeSpan bufferedDuration = TimeSpan.Zero;

        private TimeSpan lastPartialDuration = TimeSpan.Zero;

        public WhisperSpeechRecognizer(Pipeline pipeline) {
            _processor = new Lazy<WhisperProcessor>(LazyInitialize);

            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, Process, nameof(In));
            PartialOut = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(PartialOut));
            FinalOut = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(FinalOut));
            Out = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
        }

        private void OnPipelineRun(object sender, PipelineRunEventArgs args) {
            using var tokenSource = new CancellationTokenSource();
            var t = Task.Factory.StartNew(DownloadAsync, tokenSource.Token).Result;//Put on a worker thread. Otherwise, the pipeline will be blocked.
            var timeout = (int)DownloadTimeout.TotalMilliseconds;
            var succeed = t.Wait(timeout);
            if (!succeed) {
                tokenSource.Cancel();
                t.Wait();//Wait deletion to complete
                throw new TimeoutException("Download Whisper model timeout.");
            }
        }

        private async Task DownloadAsync(object state) {
            var cancellationToken = (CancellationToken)state;
            var modelType = ModelType;
            var quantizationType = QuantizationType;
            var fn = string.Join("__", "ggml", GetTypeModelFileName(modelType), GetQuantizationModelFileName(quantizationType)) + ".bin";
            modelFilename = Path.Combine(ModelDirectory, fn);
            if (ForceDownload || !File.Exists(modelFilename)) {
                try {
                    Logger?.LogInformation("Downloading Whisper model.");
                    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(modelType, quantizationType, cancellationToken);
                    using var fileWriter = File.OpenWrite(modelFilename);
                    const int bufferSize = 32 * 1024 * 1024;
                    await modelStream.CopyToAsync(fileWriter, bufferSize, cancellationToken);//TaskCanceledExpcetion will be thrown at here if canceled
                    Logger?.LogInformation("Downloaded Whisper model.");
                } catch (OperationCanceledException) {
                    File.Delete(modelFilename);//Delete incomplete file
                }
            }
            if (!LazyInitialization) {
                _ = _processor.Value;
            }
        }

        private WhisperProcessor LazyInitialize() {
            Debug.Assert(modelFilename is not null);
            if (modelFilename is null) {
                throw new InvalidOperationException();
            }
            if (!File.Exists(modelFilename)) {
                throw new FileNotFoundException("Whisper model file not exist.", modelFilename);
            }
            var code = GetLanguageCode(Language);
            var builder = WhisperFactory
                .FromPath(modelFilename)
                .CreateBuilder()
                .WithLanguage(code)
                .WithProgressHandler(OnProgress)
                .WithSegmentEventHandler(OnSegment)
                .WithProbabilities()
                .WithTokenTimestamps()
                ;
            var prompt = Prompt;
            if (!string.IsNullOrWhiteSpace(prompt)) {
                builder.WithPrompt(prompt!);
            }
            switch (SegmentationRestriction) {
                case SegmentationRestriction.OnePerWord:
                    builder.SplitOnWord();//TODO: not working?
                    break;
                case SegmentationRestriction.OnePerUtterence:
                    builder.WithSingleSegment();
                    break;
            }
            var result = builder.Build();
            Logger?.LogInformation("Whisper model is loaded.");
            return result;
        }

        private void Process((AudioBuffer, bool) frame, Envelope envelope) {
            var (data, state) = frame;

            /* Append Data */
            if (state) {
                AppendAudio(data, envelope.OriginatingTime);
                /* Post Partial */
                if (OutputPartialResults && bufferedDuration - lastPartialDuration >= PartialEvalueationInverval) {
                    lastPartialDuration = bufferedDuration;
                    ProcessAndPost(isFinal: false);
                }
                return;
            }

            /* Post Final*/
            lastPartialDuration = TimeSpan.Zero;
            ProcessAndPost(isFinal: true);
        }

        private void AppendAudio(AudioBuffer data, DateTime timestamp) {
            var inputTimeMode = InputTimestampMode;

            /* Check Format */
            Debug.Assert(timestamp.Kind == DateTimeKind.Utc);
            if (!data.HasValidData) {
                return;
            }
            if (data.Format is not { FormatTag: WaveFormatTag.WAVE_FORMAT_PCM, SamplesPerSec: 16_000, }) {//AudioResampler is platform dependent, so we are not silently resample here
                throw new Exception("Please use 16kHz PCM audio as Whisper's input.");
            }
            if (data.Format.BitsPerSample != 16) {
                throw new Exception("Only 16-bit PCM audio is currently supported.");//TODO: 24-bit on demand
            }
            if (_sections.Count > 0) {
                var format = _sections[0].Buffer.Format;
                if (!data.Format.Equals(format)) {
                    throw new Exception("Audio format mismatch.");
                }
            }

            var newSection = new Section(data.DeepClone(), timestamp, data.Duration);

            /* Fill Gap */
            if (_sections.Count > 0) {
                var last = _sections.Last();
                var blankTime = inputTimeMode switch {
                    TimestampMode.AtEnd => (newSection.OriginatingTime - newSection.Buffer.Duration) - last.OriginatingTime,
                    TimestampMode.AtStart => newSection.OriginatingTime - (last.OriginatingTime + last.Buffer.Duration),
                    _ => throw new InvalidOperationException(),
                };
                var format = _sections[0].Buffer.Format;
                var missedSamples = (int)(blankTime.TotalSeconds * format.SamplesPerSec);
                if (missedSamples > GapSampleThreshold) {
                    var factor = format.Channels * format.BitsPerSample / 8;
                    var gapBufferLength = missedSamples * factor;
                    var gap = new AudioBuffer(gapBufferLength, format);//TODO: mark in Section only, do not actually allocate memory
                    var gapTimestamp = inputTimeMode switch {
                        TimestampMode.AtEnd => last.OriginatingTime + gap.Duration,
                        TimestampMode.AtStart => newSection.OriginatingTime - gap.Duration,
                        _ => throw new InvalidOperationException(),
                    };
                    var gapSection = new Section(gap, gapTimestamp, gap.Duration);

                    _sections.Add(gapSection);
                    bufferedDuration += gapSection.Buffer.Duration;
                }
            }

            _sections.Add(newSection);
            bufferedDuration += newSection.Buffer.Duration;

            Debug.Assert(bufferedDuration <= TimeSpan.FromSeconds(30));//TODO: we haven't tested what will happen if we feed more than 30 seconds of audio
        }

        private void ProcessAndPost(bool isFinal) {
            if (_sections.Count <= 0) {
                return;
            }
            var inputTimeMode = InputTimestampMode;
            var outputTimeMode = OutputTimestampMode;
            var processor = _processor.Value;//Lazy initialize, but before data pre-processing
            var format = _sections[0].Buffer.Format;
            var factor = format.Channels * format.BitsPerSample / 8;
            var size = _sections.Sum(s => s.Buffer.Length) / factor;
            var samples = ArrayPool<float>.Shared.Rent(size);
            try {
                /* Merge */
                var sampleOffset = 0;
                foreach (var section in _sections) {
                    var bufferOffset = 0;
                    while (bufferOffset < section.Buffer.Length) {
                        var channelIdx = 0;
                        long sum = 0;
                        while (channelIdx < section.Buffer.Format.Channels) {
                            switch (section.Buffer.Format.BitsPerSample) {
                                case 16:
                                    sum += BitConverter.ToInt16(section.Buffer.Data, bufferOffset);
                                    bufferOffset += 2;
                                    break;
                                default:
                                    throw new InvalidOperationException("Not supported bit-depth.");
                            }
                            channelIdx += 1;
                        }
                        samples[sampleOffset] = sum / (float)section.Buffer.Format.Channels / (short.MaxValue + 1);
                        sampleOffset += 1;
                    }
                    Debug.Assert(bufferOffset == section.Buffer.Length);
                }
                Debug.Assert(sampleOffset == size);

                /* Process */
                var valid = samples.AsSpan(0, size);
                processor.Process(valid);

                /* Output */
                if (_segments.Count == 0) {
                    return;
                }
                var firstSection = _sections.First();
                Debug.Assert(Math.Abs(bufferedDuration.TotalMilliseconds - _sections.Aggregate(TimeSpan.Zero, (v, s) => v + s.Buffer.Duration).TotalMilliseconds) < 1);
                foreach (var segment in _segments) {
                    /* Basic Info */
                    var text = segment.Text;
                    var confidence = segment.Probability;
                    var actualEnd = segment.End > bufferedDuration ? bufferedDuration : segment.End;//Input is padded to 30 seconds, so the end time may be larger than the actual end time
                    var duration = actualEnd - segment.Start;
                    AudioBuffer? audio;
                    if (!OutputAudio) {
                        audio = null;
                    } else {
                        var audioBuffer = SegmentAudioBuffer(segment, format, _sections);
                        audio = new AudioBuffer(audioBuffer, format);
                    }
                    var result = new StreamingSpeechRecognitionResult(isFinal, text, confidence, Enumerable.Empty<SpeechRecognitionAlternate>(), audio, duration);
                    /* Timestamp */
                    var timestamp = (inputTimeMode switch {
                        TimestampMode.AtStart => firstSection.OriginatingTime,
                        TimestampMode.AtEnd => firstSection.OriginatingTime - firstSection.Duration,
                        _ => throw new InvalidOperationException(),
                    }) + (outputTimeMode switch {
                        TimestampMode.AtStart => segment.Start,
                        TimestampMode.AtEnd => actualEnd,
                        _ => throw new InvalidOperationException(),
                    });
                    switch (isFinal) {
                        case false:
                            SafePost(PartialOut, result, timestamp);
                            break;
                        case true:
                            SafePost(FinalOut, result, timestamp);
                            break;
                    }
                    SafePost(Out, result, timestamp);
                }
            } finally {
                ArrayPool<float>.Shared.Return(samples);
                if (isFinal) {
                    _sections.Clear();
                    bufferedDuration = TimeSpan.Zero;
                }
                _segments.Clear();
            }
        }

        private void OnSegment(SegmentData segment) {
            _segments.Add(segment);
        }

        private void OnProgress(int progress) {
            Progress = progress;//What is this? 100 times of 0.01sec units? https://github.com/ggerganov/whisper.cpp/blob/2f52783a080e8955e80e4324fed73e2f906bb80c/whisper.cpp#L4270C84-L4270C84
        }

        private static byte[] SegmentAudioBuffer(SegmentData segment, WaveFormat format, IReadOnlyList<Section> sections) {
            var factor = format.Channels * format.BitsPerSample / 8;

            /* Buffer Length */
            var sectionIdx = 0;
            var sectionStartTime = TimeSpan.Zero;
            var length = 0L;
            while (true) {
                if (sectionIdx >= sections.Count) {
                    break;
                }
                var section = sections[sectionIdx].Buffer;
                var sectionEndTime = sectionStartTime + section.Duration;
                if (segment.Start <= sectionEndTime && sectionStartTime < segment.End) {//has overlap
                    long startIdx;
                    if (segment.Start <= sectionStartTime) {
                        startIdx = 0;
                    } else {
                        startIdx = (long)((segment.Start - sectionStartTime).TotalSeconds * format.SamplesPerSec) * factor;
                    }
                    long endIdx;
                    if (sectionEndTime <= segment.End) {
                        endIdx = section.Length;
                    } else {
                        endIdx = (long)((segment.End - sectionStartTime).TotalSeconds * format.SamplesPerSec) * factor;
                    }
                    length += endIdx - startIdx;
                } else if (sectionStartTime >= segment.End) {
                    break;
                }
                sectionIdx += 1;
                sectionStartTime = sectionEndTime;
            }

            var result = new byte[length];

            /* Copy */
            sectionIdx = 0;
            sectionStartTime = TimeSpan.Zero;
            var offset = 0L;
            while (true) {
                if (sectionIdx >= sections.Count) {
                    Debug.Assert(false);//should not reach here
                    break;
                }
                var section = sections[sectionIdx].Buffer;
                var sectionEndTime = sectionStartTime + section.Duration;
                if (segment.Start <= sectionEndTime && sectionStartTime < segment.End) {//has overlap
                    long startIdx;
                    if (segment.Start <= sectionStartTime) {
                        startIdx = 0;
                    } else {
                        startIdx = (long)((segment.Start - sectionStartTime).TotalSeconds * format.SamplesPerSec) * factor;
                    }
                    long endIdx;
                    if (sectionEndTime <= segment.End) {
                        endIdx = section.Length;
                    } else {
                        endIdx = (long)((segment.End - sectionStartTime).TotalSeconds * format.SamplesPerSec) * factor;
                    }
                    var bytes = endIdx - startIdx;
                    Array.Copy(section.Data, startIdx, result, offset, bytes);
                    offset += bytes;
                    if (offset >= length) {
                        Debug.Assert(offset == length);
                        break;
                    }
                } else if (sectionStartTime >= segment.End) {
                    Debug.Assert(false);//should not reach here
                    break;
                }
                sectionIdx += 1;
                sectionStartTime = sectionEndTime;
            }

            return result;
        }

        private void SafePost(Emitter<IStreamingSpeechRecognitionResult> emitter, IStreamingSpeechRecognitionResult data, DateTime timestamp) {
            var minTimestamp = emitter.LastEnvelope.OriginatingTime + TimeSpan.FromMilliseconds(1);
            if (timestamp < minTimestamp) {
                timestamp = minTimestamp;
            }
            emitter.Post(data, timestamp);
        }

        private static string GetTypeModelFileName(GgmlType modelType) => modelType switch { 
            GgmlType.Tiny => "tiny__v1",
            GgmlType.TinyEn => "tiny_en__v1",
            GgmlType.Base => "base__v1",
            GgmlType.BaseEn => "base_en__v1",
            GgmlType.Small => "small__v1",
            GgmlType.SmallEn => "small_en__v1",
            GgmlType.Medium => "medium__v1",
            GgmlType.MediumEn => "medium_en__v1",
            GgmlType.LargeV1 => "large__v1",
            GgmlType.LargeV2 => "large__v2",
            GgmlType.LargeV3 => "large__v3",
            _ => throw new InvalidOperationException(),
        };

        private static string GetQuantizationModelFileName(QuantizationType quantizationType) => quantizationType switch {
            QuantizationType.NoQuantization => "classic",
            QuantizationType.Q4_0 => "q4_0",
            QuantizationType.Q4_1 => "q4_1",
            QuantizationType.Q5_0 => "q5_0",
            QuantizationType.Q5_1 => "q5_1",
            QuantizationType.Q8_0 => "q8_0",
            _ => throw new InvalidOperationException(),
        };

        private static string GetLanguageCode(Language language) => language switch {//Generated, not tested
            Language.NotSet => "auto",
            Language.Afrikaans => "af",
            Language.Arabic => "ar",
            Language.Armenian => "hy",
            Language.Azerbaijani => "az",
            Language.Belarusian => "be",
            Language.Bosnian => "bs",
            Language.Bulgarian => "bg",
            Language.Catalan => "ca",
            Language.Chinese => "zh",
            Language.Croatian => "hr",
            Language.Czech => "cs",
            Language.Danish => "da",
            Language.Dutch => "nl",
            Language.English => "en",
            Language.Estonian => "et",
            Language.Finnish => "fi",
            Language.French => "fr",
            Language.Galician => "gl",
            Language.German => "de",
            Language.Greek => "el",
            Language.Hebrew => "he",
            Language.Hindi => "hi",
            Language.Hungarian => "hu",
            Language.Icelandic => "is",
            Language.Indonesian => "id",
            Language.Italian => "it",
            Language.Japanese => "ja",
            Language.Kannada => "kn",
            Language.Kazakh => "kk",
            Language.Korean => "ko",
            Language.Latvian => "lv",
            Language.Lithuanian => "lt",
            Language.Macedonian => "mk",
            Language.Malay => "ms",
            Language.Marathi => "mr",
            Language.Maori => "mi",
            Language.Nepali => "ne",
            Language.Norwegian => "no",
            Language.Persian => "fa",
            Language.Polish => "pl",
            Language.Portuguese => "pt",
            Language.Romanian => "ro",
            Language.Russian => "ru",
            Language.Serbian => "sr",
            Language.Slovak => "sk",
            Language.Slovenian => "sl",
            Language.Spanish => "es",
            Language.Swahili => "sw",
            Language.Swedish => "sv",
            Language.Tagalog => "tl",
            Language.Tamil => "ta",
            Language.Thai => "th",
            Language.Turkish => "tr",
            Language.Ukrainian => "uk",
            Language.Urdu => "ur",
            Language.Vietnamese => "vi",
            Language.Welsh => "cy",
            _ => throw new InvalidOperationException(),
        };

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            if (_processor.IsValueCreated) {
                _processor.Value.Dispose();
            }
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

        private readonly struct Section {

            public readonly AudioBuffer Buffer;

            public readonly DateTime OriginatingTime;

            public readonly TimeSpan Duration;

            public Section(AudioBuffer buffer, DateTime originatingTime, TimeSpan duration) {
                Buffer = buffer;
                OriginatingTime = originatingTime;
                Duration = duration;
            }
        }
    }
}
