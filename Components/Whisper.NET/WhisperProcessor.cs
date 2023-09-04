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
using Whisper.net;
using Whisper.net.Ggml;

namespace OpenSense.Components.Whisper.NET {

    public sealed class WhisperProcessor : IConsumerProducer<(AudioBuffer, bool), IStreamingSpeechRecognitionResult>, IDisposable, INotifyPropertyChanged {

        /// <remarks>
        /// This value should be set as low as possible, while still being high enough to ensure that gap filling is never triggered when the delivery policy is set to Unlimited.
        /// </remarks>
        private const int GapSampleThreshold = 1;

        private readonly List<Section> _sections = new();

        private readonly List<SegmentData> _segments = new();

        private readonly Lazy<global::Whisper.net.WhisperProcessor> _processor;

        private GgmlType modelType = GgmlType.TinyEn;

        public GgmlType ModelType {
            get => modelType;
            set => SetProperty(ref modelType, value);
        }

        private QuantizationType quantizationType = QuantizationType.NoQuantization;

        public QuantizationType QuantizationType {
            get => quantizationType;
            set => SetProperty(ref quantizationType, value);
        }

        private string modelDirectory = "";

        public string ModelDirectory {
            get => modelDirectory;
            set => SetProperty(ref modelDirectory, value);
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

        private AudioBufferTimestampMode inputTimestampMode = AudioBufferTimestampMode.AtEnd;//\psi convention

        public AudioBufferTimestampMode InputTimestampMode {
            get => inputTimestampMode;
            set => SetProperty(ref inputTimestampMode, value);
        }

        private bool outputAudio = false;

        public bool OutputAudio {
            get => outputAudio;
            set => SetProperty(ref outputAudio, value);
        }

        public ILogger? Logger { get; set; }

        public Receiver<(AudioBuffer, bool)> In { get; }

        public Emitter<IStreamingSpeechRecognitionResult> Out { get; }

        private string? modelFilename;

        public WhisperProcessor(Pipeline pipeline) {
            _processor = new Lazy<global::Whisper.net.WhisperProcessor>(LazyInitialize);

            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, Process, nameof(In));
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
            modelFilename = Path.Combine(ModelDirectory, GetModelFileName(modelType, quantizationType));
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
        }

        private global::Whisper.net.WhisperProcessor LazyInitialize() {
            Debug.Assert(modelFilename is not null);
            if (modelFilename is null) {
                throw new InvalidOperationException();
            }
            if (!File.Exists(modelFilename)) {
                throw new FileNotFoundException("Whisper model file not exist.", modelFilename);
            }
            var builder = WhisperFactory
                .FromPath(modelFilename)
                .CreateBuilder()
                .WithSegmentEventHandler(OnSegment)
                .WithProbabilities()
                .WithLanguage("en")
                .WithTokenTimestamps()
                .WithSingleSegment()
                ;
            var result = builder.Build();
            return result;
        }

        private void Process((AudioBuffer, bool) frame, Envelope envelope) {
            var (data, state) = frame;
            var inputTimeMode = InputTimestampMode;

            /* Append Data */
            if (state) {
                /* Check Format */
                Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
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

                var newSection = new Section(data.DeepClone(), envelope.OriginatingTime, data.Duration);

                /* Fill Gap */
                if (_sections.Count > 0) {
                    var last = _sections.Last();
                    var blankTime = inputTimeMode switch {
                        AudioBufferTimestampMode.AtEnd => (newSection.OriginatingTime - newSection.Buffer.Duration) - last.OriginatingTime,
                        AudioBufferTimestampMode.AtStart => newSection.OriginatingTime - (last.OriginatingTime + last.Buffer.Duration),
                        _ => throw new InvalidOperationException(),
                    };
                    var format = _sections[0].Buffer.Format;
                    var missedSamples = (int)(blankTime.TotalSeconds * format.SamplesPerSec);
                    if (missedSamples > GapSampleThreshold) {
                        var factor = format.Channels * format.BitsPerSample / 8;
                        var gap = new AudioBuffer(missedSamples * factor, format);
                        var timestamp = inputTimeMode switch {
                            AudioBufferTimestampMode.AtEnd => last.OriginatingTime + gap.Duration,
                            AudioBufferTimestampMode.AtStart => newSection.OriginatingTime - gap.Duration,
                            _ => throw new InvalidOperationException(),
                        };
                        var gapSection = new Section(gap, timestamp, gap.Duration);
                        _sections.Add(gapSection);
                    }
                }

                _sections.Add(newSection);

                Debug.Assert(_sections.Aggregate(TimeSpan.Zero, (v, s) => v + s.Buffer.Duration) <= TimeSpan.FromSeconds(30));//TODO: we haven't tested what will happen if we feed more than 30 seconds of audio
                return;
            }

            /* Process Data */
            if (_sections.Count > 0) {
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
                    Debug.Assert(_segments.Count == 0 || _segments.Count == 1);//We set WithSingleSegment()
                    foreach (var segment in _segments) {
                        /* Basic Info */
                        const bool isFinal = true;
                        var text = segment.Text;
                        var confidence = segment.Probability;
                        var duration = segment.End - segment.Start;
                        AudioBuffer? audio;
                        if (!OutputAudio) {
                            audio = null;
                        } else {
                            var audioBuffer = SegmentAudioBuffer(segment, format, _sections);
                            audio = new AudioBuffer(audioBuffer, format);
                        }
                        var result = new StreamingSpeechRecognitionResult(isFinal, text, confidence, Enumerable.Empty<SpeechRecognitionAlternate>(), audio, duration);
                        /* Timestamp */
                        var timestamp = inputTimeMode switch {
                            AudioBufferTimestampMode.AtEnd => (_sections[0].OriginatingTime - _sections[0].Duration) + _segments[0].Start,
                            AudioBufferTimestampMode.AtStart => _sections[0].OriginatingTime + _segments[0].Start,
                            _ => throw new InvalidOperationException(),
                        };
                        Out.Post(result, timestamp);
                    }
                    _segments.Clear();
                } finally {
                    ArrayPool<float>.Shared.Return(samples);
                    _sections.Clear();
                }
            }
        }

        private void OnSegment(SegmentData segment) {
            _segments.Add(segment);
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

        private static string GetModelFileName(GgmlType modelType, QuantizationType quantizationType) => (modelType, quantizationType) switch { 
            (GgmlType.TinyEn, QuantizationType.NoQuantization) => "ggml-tiny.en.bin",
            (GgmlType.BaseEn, QuantizationType.NoQuantization) => "ggml-base.en.bin",
            (GgmlType.SmallEn, QuantizationType.NoQuantization) => "ggml-small.en.bin",
            (GgmlType.MediumEn, QuantizationType.NoQuantization) => "ggml-medium.en.bin",
            (GgmlType.Large, QuantizationType.NoQuantization) => "ggml-large.bin",
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
