using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Speech;
using OpenSense.Components.Audio;
using OpenSense.Components.Whisper.NET;
using Whisper.net.Ggml;

namespace Whisper.NET.Tests {
    public sealed class WhisperTests {

        public WhisperTests() {
            
        }

        [Fact]
        public void TestOver30SecondsMerge() {
            var pipeline = Pipeline.Create(deliveryPolicy: DeliveryPolicy.Unlimited);
            var waveSource = new WaveFileAudioSource(pipeline, "Resources/Wonderland_Azure_Jenny_16kHz_75sec.wav");
            var mockVad = waveSource.Process<AudioBuffer, bool>((_, envelope, emitter) => {
                //Whisper component has a promblem that it can't post all the results if the last VAD is not false because \Psi does not support detecting the end of a stream.
                var value = envelope.OriginatingTime - pipeline.StartTime <= TimeSpan.FromSeconds(74);
                emitter.Post(value, envelope.OriginatingTime);
            });
            var input = waveSource.Join(mockVad);
            var whisper = new WhisperSpeechRecognizer(pipeline) {
                ModelDirectory = "",
                ModelType = GgmlType.BaseEn,
                QuantizationType = QuantizationType.Q8_0,
                ForceDownload = false,
                DownloadTimeout = TimeSpan.FromMinutes(1),
                LazyInitialization = false,
                Language = Language.English,
                Prompt = "",
                SegmentationRestriction = SegmentationRestriction.OnePerUtterence,
                InputTimestampMode = TimestampMode.AtEnd,
                OutputTimestampMode = TimestampMode.AtEnd,
                OutputPartialResults = false,
                PartialEvalueationInverval = TimeSpan.FromSeconds(1),
                OutputAudio = false,
            };
            input.PipeTo(whisper);
            var outputs = new List<StreamingSpeechRecognitionResult>();
            whisper.Out.Do(result => {
                var converted = (StreamingSpeechRecognitionResult)result;
                outputs.Add(converted);
            });
            var nonRealtimeReplay = ReplayDescriptor.ReplayAll;
            pipeline.Run(nonRealtimeReplay);
            pipeline.Dispose();
            Assert.True(outputs.Count == 1);
        }

        [Fact]
        public void TestMergeDescriptions() {
            var pipeline = Pipeline.Create(deliveryPolicy: DeliveryPolicy.Unlimited);
            var waveSource = new WaveFileAudioSource(pipeline, "Resources/BlankAudio_16kHz_44sec.wav");
            var mockVad = waveSource.Process<AudioBuffer, bool>((_, envelope, emitter) => {
                //Whisper component has a promblem that it can't post all the results if the last VAD is not false because \Psi does not support detecting the end of a stream.
                var value = envelope.OriginatingTime - pipeline.StartTime <= TimeSpan.FromSeconds(40);
                emitter.Post(value, envelope.OriginatingTime);
            });
            var input = waveSource.Join(mockVad);
            var whisper = new WhisperSpeechRecognizer(pipeline) {
                ModelDirectory = "",
                ModelType = GgmlType.BaseEn,
                QuantizationType = QuantizationType.Q8_0,
                ForceDownload = false,
                DownloadTimeout = TimeSpan.FromMinutes(1),
                LazyInitialization = false,
                Language = Language.English,
                Prompt = "",
                SegmentationRestriction = SegmentationRestriction.OnePerUtterence,
                InputTimestampMode = TimestampMode.AtEnd,
                OutputTimestampMode = TimestampMode.AtEnd,
                OutputPartialResults = false,
                PartialEvalueationInverval = TimeSpan.FromSeconds(1),
                OutputAudio = false,
            };
            input.PipeTo(whisper);
            var outputs = new List<StreamingSpeechRecognitionResult>();
            whisper.Out.Do(result => {
                var converted = (StreamingSpeechRecognitionResult)result;
                outputs.Add(converted);
            });
            var nonRealtimeReplay = ReplayDescriptor.ReplayAll;
            pipeline.Run(nonRealtimeReplay);
            pipeline.Dispose();
            Assert.True(outputs.Count == 1 && outputs.Single().Text == "[BLANK_AUDIO]");//Only one [BLANK_AUDIO]
        }
    }
}
