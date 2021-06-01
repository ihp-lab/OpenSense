using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using Microsoft.Psi.Media;
using Microsoft.Psi.Media_Interop;

namespace OpenSense.Component.Media.Writer {
    /// <summary>
    /// This component will write multiple mp4 files when the activity indicator is on.
    /// </summary>
    public class ActivityControlledMpeg4FilesWriter : Subpipeline, IConsumer<(Shared<Image>, bool)>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private readonly Func<Envelope, string> _filenameGenerator;
        private readonly Mpeg4WriterConfiguration _configuration;

        private Connector<(Shared<Image>, bool)> InConnector;

        private Connector<AudioBuffer> AudioInConnector;

        public Receiver<(Shared<Image>, bool)> In => InConnector.In;

        public Receiver<AudioBuffer> AudioIn => AudioInConnector.In;

        private bool pipelineEverStarted = false;
        private MP4Writer writer;
        private bool lastActivity = false;

        public ActivityControlledMpeg4FilesWriter(Pipeline pipeline, Func<Envelope, string> filenameGenerator, Mpeg4WriterConfiguration configuration) : base(pipeline) {
            _filenameGenerator = filenameGenerator ?? throw new ArgumentNullException(nameof(filenameGenerator));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            InConnector = CreateInputConnectorFrom<(Shared<Image>, bool)>(this, nameof(In));
            AudioInConnector = CreateInputConnectorFrom<AudioBuffer>(this, nameof(AudioIn));

            var videoTime = InConnector.Select((_, e) => e.OriginatingTime);
            var audioTime = AudioInConnector.Select((_, e) => e.OriginatingTime);
            var time = videoTime.Zip(audioTime); Debug.Assert(configuration.ContainsAudio);//TODO: if no audio stream is connected, all video message will be buffered.
            var j1 = time.Join(InConnector, Reproducible.ExactOrDefault<(Shared<Image>, bool)>());
            var j2 = j1.Join(AudioInConnector, Reproducible.ExactOrDefault<AudioBuffer>());
            var ordered = j2.Select(data => ((data.Item2, data.Item3), data.Item4));
            ordered.Do(Process);

            pipeline.PipelineRun += (s, e) => OnPipelineRun();
        }

        public override void Dispose() {
            DisposeWriter();
            if (pipelineEverStarted) {
                MP4Writer.Shutdown();
            }
            base.Dispose();
        }

        private void OnPipelineRun() {
            MP4Writer.Startup();
            pipelineEverStarted = true;
        }

        private void EnsureWriter(Envelope envelope) {
            if (writer != null) {
                return;
            }
            var filename = _filenameGenerator(envelope);
            writer = new MP4Writer();
            var config = new MP4WriterConfiguration() { //_configuration.Config is an internal property, can not be accessed here
                imageWidth = _configuration.ImageWidth,
                imageHeight = _configuration.ImageHeight,
                frameRateNumerator = _configuration.FrameRateNumerator,
                frameRateDenominator = _configuration.FrameRateDenominator,
                targetBitrate = _configuration.TargetBitrate,
                pixelFormat = (int)_configuration.PixelFormat,
                containsAudio = _configuration.ContainsAudio,
                bitsPerSample = _configuration.AudioBitsPerSample,
                samplesPerSecond = _configuration.AudioSamplesPerSecond,
                numChannels = _configuration.AudioChannels,
            };
            writer.Open(filename, config);
        }

        private void DisposeWriter() {
            if (writer is null) {
                return;
            }
            writer.Close();
            ((IDisposable)writer).Dispose(); // Cast to IDisposable to suppress false CA2213 warning
            writer = null;
        }

        private void Process(((Shared<Image>, bool), AudioBuffer) data, Envelope envelope) {//in chronological order
            var ((video, activity), audio) = data;
            if (video != null) {
                if (activity) {// only when video is not null, activity has a valid value
                    EnsureWriter(envelope);
                    WriteImage(video, envelope);
                } else {
                    DisposeWriter();
                }
                lastActivity = activity;
            }
            if (audio.HasValidData) {
                if (lastActivity) {
                    Debug.Assert(writer != null);
                    WriteAudio(audio, envelope);
                }
            }
        }

        private void WriteImage(Shared<Image> image, Envelope e) {
            writer.WriteVideoFrame(e.OriginatingTime.Ticks, image.Resource.ImageData, (uint)image.Resource.Width, (uint)image.Resource.Height, (int)image.Resource.PixelFormat);
        }

        private void WriteAudio(AudioBuffer audioBuffer, Envelope e) {
            IntPtr waveFmtPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal((int)WaveFormat.MarshalSizeOf(audioBuffer.Format) + sizeof(int));
            WaveFormat.MarshalToPtr(audioBuffer.Format, waveFmtPtr);
            IntPtr audioData = System.Runtime.InteropServices.Marshal.AllocHGlobal(audioBuffer.Length);
            System.Runtime.InteropServices.Marshal.Copy(audioBuffer.Data, 0, audioData, audioBuffer.Length);
            writer.WriteAudioSample(e.OriginatingTime.Ticks, audioData, (uint)audioBuffer.Length, waveFmtPtr);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(waveFmtPtr);
            System.Runtime.InteropServices.Marshal.FreeHGlobal(audioData);
        }
    }
}
