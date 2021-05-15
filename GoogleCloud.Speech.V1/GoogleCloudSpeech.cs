using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Speech.V1;
using Google.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using Microsoft.Psi.Speech;

namespace OpenSense.Component.GoogleCloud.Speech.V1 {

    public class GoogleCloudSpeech : IConsumerProducer<(AudioBuffer, bool), IStreamingSpeechRecognitionResult> {

        private static readonly IList<SpeechRecognitionAlternate> EmptyAlternatives = new List<SpeechRecognitionAlternate> {
           new SpeechRecognitionAlternate(text: "", confidence: 1.0),
        };

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<(AudioBuffer, bool)> In { get; private set; }

        public Emitter<IStreamingSpeechRecognitionResult> Out { get; private set; }

        public Emitter<AudioBuffer> Audio { get; private set; }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private bool atMostOneFinalResultEachVadSession = false;

        public bool AtMostOneFinalResultEachVadSession {
            get => atMostOneFinalResultEachVadSession;
            set => SetProperty(ref atMostOneFinalResultEachVadSession, value);
        }

        private string partialFinalResultDelimiter = ", ";

        /// <summary>
        /// This delimiter will be used to join partial final results within a VAD session.
        /// The default value is ", ".
        /// </summary>
        /// <remarks>
        /// Effective when <see cref="AtMostOneFinalResultEachVadSession"/> is enabled.
        /// </remarks>
        public string PartialFinalResultDelimiter {
            get => partialFinalResultDelimiter;
            set => SetProperty(ref partialFinalResultDelimiter, value);
        }

        private string languateCode = "en-US";

        public string LanguageCode {
            get => languateCode;
            set => SetProperty(ref languateCode, value);
        }

        private bool separateRecognitionPerChannel = false;

        /// <summary>
        /// If not set to true, only the first channel is recognized.
        /// </summary>
        public bool SeparateRecognitionPerChannel {
            get => separateRecognitionPerChannel;
            set => SetProperty(ref separateRecognitionPerChannel, value);
        }

        private bool postInterimResults = true;

        public bool PostInterimResults {
            get => postInterimResults;
            set => SetProperty(ref postInterimResults, value);
        }

        private bool addDurationToOutputTime = false;

        public bool AddDurationToOutputTime {
            get => addDurationToOutputTime;
            set => SetProperty(ref addDurationToOutputTime, value);
        }

        private string jsonCredentials;
        private SpeechClient client;
        private SpeechClient.StreamingRecognizeStream stream;
        private CancellationTokenSource streamCancellationTokenSource;
        private Task responseTask;
        private DateTime? startTime;
        private DateTime maxPostTime = DateTime.MinValue;
        private IList<SpeechRecognitionAlternate> bufferedAlternatives = EmptyAlternatives;

        private bool ClientInitialized => responseTask != null && !responseTask.IsCompleted;
        private bool FormatInitialized => startTime != null;

        public GoogleCloudSpeech(Pipeline pipeline, string jsonCredentials) {
            // psi pipeline
            In = pipeline.CreateReceiver<(AudioBuffer, bool)>(this, PorcessFrames, nameof(In));
            Out = pipeline.CreateEmitter<IStreamingSpeechRecognitionResult>(this, nameof(Out));
            Audio = pipeline.CreateEmitter<AudioBuffer>(this, nameof(Audio));

            pipeline.PipelineRun += OnPipeRun;
            pipeline.PipelineCompleted += OnPipeCompleted;

            this.jsonCredentials = jsonCredentials;
        }

        private void OnPipeRun(object sender, PipelineRunEventArgs e) {

        }

        private void OnPipeCompleted(object sender, PipelineCompletedEventArgs e) {
            Stop();
        }

        private void InitializeClient() {
            Stop();

            streamCancellationTokenSource = new CancellationTokenSource();
            startTime = null;

            client = new SpeechClientBuilder() {
                JsonCredentials = jsonCredentials,
            }.Build();
            stream = client.StreamingRecognize();
            responseTask = Task.Run(ProcessResponses);
        }

        private void InitializeFormat(AudioBuffer frame, Envelope envelope) {
            startTime = envelope.OriginatingTime;
            var initReq = new StreamingRecognizeRequest() {
                StreamingConfig = new StreamingRecognitionConfig() {
                    Config = new RecognitionConfig() {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = (int)frame.Format.SamplesPerSec,
                        EnableSeparateRecognitionPerChannel = SeparateRecognitionPerChannel,
                        AudioChannelCount = frame.Format.Channels,
                        LanguageCode = LanguageCode,
                    },
                    InterimResults = PostInterimResults,
                }
            };
            stream.WriteAsync(initReq).Wait();
        }

        private void Stop() {
            stream?.WriteCompleteAsync().Wait();
            streamCancellationTokenSource?.Cancel();
            responseTask?.Wait();
            PostBufferedAsFinal();
            stream = null;
            streamCancellationTokenSource = null;
            responseTask = null;
            client = null;
        }

        private void PorcessFrames((AudioBuffer, bool) frame, Envelope envelope) {
            var (audio, active) = frame;
            if (Mute || !active) {
                Stop();
                return;
            }
            if (audio.Data.Length == 0) {
                return;
            }
            if (!ClientInitialized) {
                InitializeClient();
            }
            Trace.Assert(audio.Format.FormatTag == WaveFormatTag.WAVE_FORMAT_PCM && audio.Format.BitsPerSample == 16);//TODO: convert format silently
            if (!FormatInitialized) {
                InitializeFormat(audio, envelope);
            }
            var request = new StreamingRecognizeRequest() {
                AudioContent = ByteString.CopyFrom(audio.Data, 0, audio.Data.Length),
            };
            stream.WriteAsync(request).Wait();
            Audio.Post(audio, envelope.OriginatingTime);
        }

        private async Task ProcessResponses() {//Note: do not use ValueTask
            try {
                var responseStream = stream.GetResponseStream();
                while (!streamCancellationTokenSource.Token.IsCancellationRequested) {
                    while (await responseStream.MoveNextAsync(/*streamCancellationTokenSource.Token*/)) {
                        var response = responseStream.Current;
                        var mostStablePortion = response.Results.FirstOrDefault();
                        if (mostStablePortion is null) {
                            continue;
                        }
                        var alternatives = mostStablePortion.Alternatives
                                .Select(alt => new SpeechRecognitionAlternate(alt.Transcript, alt.Confidence))
                                .ToList();
                        var combined = CombineAlternatives(bufferedAlternatives, alternatives);
                        var bestAlternative = combined.First();
                        var restAlternatives = combined
                            .Skip(1);
                        var isFinalResult = mostStablePortion.IsFinal && !AtMostOneFinalResultEachVadSession;
                        var result = new StreamingSpeechRecognitionResult(isFinalResult, bestAlternative.Text, bestAlternative.Confidence, restAlternatives);
                        DateTime postTime;
                        if (AddDurationToOutputTime) {
                            var duration = TimeSpan.FromSeconds(mostStablePortion.ResultEndTime.Seconds) + TimeSpan.FromMilliseconds(mostStablePortion.ResultEndTime.Nanos / 1e6);
                            var resultTime = 
                            postTime = startTime.Value + duration;
                        } else {
                            postTime = startTime.Value;
                        }
                        lock (this) {
                            if (postTime <= maxPostTime) {
                                postTime = maxPostTime + TimeSpan.FromMilliseconds(1);//TimeSpan.MinValue too small
                                Debug.Assert(postTime > maxPostTime);
                            }
                            Out.Post(result, postTime);//Note: time may be inaccurate
                            maxPostTime = postTime;
                            if (AtMostOneFinalResultEachVadSession) {
                                if (mostStablePortion.IsFinal) {//only update for final results
                                    bufferedAlternatives = combined;
                                }
                            } else {
                                bufferedAlternatives = EmptyAlternatives;
                            }
                        }
                    }
                }
            } catch (Grpc.Core.RpcException ex1) when (ex1.StatusCode == Grpc.Core.StatusCode.OutOfRange) {
                ;//when google cloud does not receiving data continually
            } catch (Grpc.Core.RpcException ex2) when (ex2.StatusCode == Grpc.Core.StatusCode.Cancelled) {
                ;
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            ;//for debug
        }

        private void PostBufferedAsFinal() {
            lock (this) {
                if (bufferedAlternatives != EmptyAlternatives) {
                    var bestAlternative = bufferedAlternatives.First();
                    var restAlternatives = bufferedAlternatives
                        .Skip(1);
                    var result = new StreamingSpeechRecognitionResult(isFinal: true, bestAlternative.Text, bestAlternative.Confidence, restAlternatives);
                    var postTime = maxPostTime + TimeSpan.FromMilliseconds(1);
                    Debug.Assert(postTime > maxPostTime);
                    Out.Post(result, postTime);
                    maxPostTime = postTime;
                    bufferedAlternatives = EmptyAlternatives;
                } 
            }
        }

        private IList<SpeechRecognitionAlternate> CombineAlternatives(IList<SpeechRecognitionAlternate> a, IList<SpeechRecognitionAlternate> b) {
            var altsWithConfidence = new List<SpeechRecognitionAlternate>();
            var altsWithoutConfidence = new List<SpeechRecognitionAlternate>();
            Trace.Assert(a.Any());
            Trace.Assert(b.Any());
            foreach (var left in a) {
                foreach (var right in b) {
                    Trace.Assert(!string.IsNullOrWhiteSpace(right.Text));
                    var altText = string.IsNullOrWhiteSpace(left.Text) ? right.Text.Trim() : (left.Text + PartialFinalResultDelimiter + right.Text.Trim());
                    var altConfidence = left.Confidence * right.Confidence;
                    var alt = new SpeechRecognitionAlternate(altText, altConfidence);
                    if (altConfidence != null) {
                        altsWithConfidence.Add(alt);
                    } else {
                        altsWithoutConfidence.Add(alt);
                    }
                }
            }
            var result = altsWithConfidence
                .OrderByDescending(alt => alt.Confidence)
                .Concat(altsWithoutConfidence)
                .Where(alt => !string.IsNullOrWhiteSpace(alt.Text))
                .GroupBy(alt => alt.Text)
                .Select(group => {
                    var confidence = group.Select(alt => alt.Confidence).Aggregate((a, b) => {
                        switch ((a, b)) {
                            case (null, null):
                                return null;
                            case (null, double bb):
                                return bb;
                            case (double aa, null):
                                return aa;
                            case (double aa, double bb):
                                return Math.Max(aa, bb);
                        }
                    });
                    return new SpeechRecognitionAlternate(text: group.Key, confidence: confidence);
                })
                .ToList();
            Trace.Assert(result.Any());
            return result;
        }
    }
}
