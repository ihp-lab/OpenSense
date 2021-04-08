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

        private string jsonCredentials;
        private SpeechClient client;
        private SpeechClient.StreamingRecognizeStream stream;
        private CancellationTokenSource streamCancellationTokenSource;
        private Task responseTask;
        private DateTime? startTime;
        private DateTime maxPostTime = DateTime.MinValue;

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
                        var bestAlternative = mostStablePortion.Alternatives.First();
                        var restAlternatives = mostStablePortion.Alternatives
                            .Skip(1)
                            .Select(a => new SpeechRecognitionAlternate(a.Transcript, a.Confidence));
                        var duration = TimeSpan.FromSeconds(mostStablePortion.ResultEndTime.Seconds) + TimeSpan.FromMilliseconds(mostStablePortion.ResultEndTime.Nanos / 1e6);
                        var result = new StreamingSpeechRecognitionResult(mostStablePortion.IsFinal, bestAlternative.Transcript, bestAlternative.Confidence, restAlternatives);
                        var resultTime = startTime.Value + duration;
                        var postTime = resultTime;
                        if (postTime <= maxPostTime) {
                            postTime = maxPostTime + TimeSpan.FromMilliseconds(1);//TimeSpan.MinValue too small
                            Debug.Assert(postTime > maxPostTime);
                        }
                        Out.Post(result, postTime);//Note: time may be inaccurate
                        maxPostTime = postTime;
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
    }
}
