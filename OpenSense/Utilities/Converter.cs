namespace OpenSense.Utilities {
    using System;
    using System.Windows;

    using Microsoft.Psi;
    using Microsoft.Psi.Data;
    using Microsoft.Psi.Media;
    using Microsoft.Psi.Audio;
    using Microsoft.Psi.Imaging;

    using NAudio.Wave;

    /// <summary>
    /// Converter class.
    /// </summary>
    public class Converter {
        /// <summary>
        /// Initializes a new instance of the <see cref="Converter"/> class.
        /// </summary>
        public Converter() {
            storeName = "OpenSense";
            storePath = "./data/david/OpenSense.0000";
        }

        /// <summary>
        /// Convert binary audio/video to mpeg4/wav.
        /// </summary>
        public void ConvertBinaryAudioVideo() {
            pipeline = Pipeline.Create();
            pipeline.PipelineCompleted += PipelineCompletedMethod;

            localStore = Store.Open(pipeline, storeName, storePath);
            //videoSource = localStore.OpenStream<Shared<Image>>("Video");
            encodedImageSource = localStore.OpenStream<Shared<EncodedImage>>("Image");
            audioSource = localStore.OpenStream<AudioBuffer>("Audio");

            mpeg4Writer = new Mpeg4Writer(pipeline, storePath + "/output.mp4", new Mpeg4WriterConfiguration {
                ImageWidth = 640,
                ImageHeight = 480,
                PixelFormat = PixelFormat.BGR_24bpp,
                FrameRateNumerator = 30,
                FrameRateDenominator = 1,
                TargetBitrate = 10000000,
                ContainsAudio = false,
                AudioBitsPerSample = 16,
                AudioSamplesPerSecond = 16000,
                AudioChannels = 1
            });

            waveWriter = new Microsoft.Psi.Audio.WaveFileWriter(pipeline, storePath + "/output.wav");

            encodedImageSource.Out.Where((encodedImage, envelope) => TimeCompareImage(encodedImage, envelope) > 0).Select((encodedImage, envelope) => EncodedImageToImage(encodedImage, envelope)).PipeTo(mpeg4Writer.ImageIn);
            //audioSource.Out.Where((audio, envelope) => TimeCompareAudio(audio, envelope) > 0).PipeTo(mpeg4Writer.AudioIn);

            audioSource.Out.Where((audio, envelope) => TimeCompareAudio(audio, envelope) > 0).PipeTo(waveWriter.In);

            //videoSource.Out.PipeTo(mpeg4Writer.ImageIn);
            //audioSource.Out.PipeTo(mpeg4Writer.AudioIn);
            //audioSource.Out.PipeTo(waveWriter.In);

            pipeline.RunAsync();
        }

        /// <summary>
        /// Convert encoded image to image.
        /// </summary>
        public Shared<Image> EncodedImageToImage(Shared<EncodedImage> encodedImage, Envelope envelope) {
            var decoder = new ImageFromStreamDecoder();//this is a platform specific decoder
            var image = encodedImage.Resource.Decode(decoder);
            var sharedImage = ImagePool.GetOrCreate(encodedImage.Resource.Width, encodedImage.Resource.Height, image.PixelFormat);
            sharedImage.Resource.CopyFrom(image.ImageData);
            return sharedImage;
        }

        /// <summary>
        /// Time compare for image.
        /// </summary>
        public int TimeCompareImage(Shared<EncodedImage> image, Envelope envelope) {
            DateTime targetTime = new DateTime(2019, 8, 22, 23, 30, 29, 968);
            int diff = envelope.OriginatingTime.CompareTo(targetTime);

            Console.WriteLine("Image: " + envelope.OriginatingTime);

            return diff;
        }

        /// <summary>
        /// Time compare for audio.
        /// </summary>
        public int TimeCompareAudio(AudioBuffer audio, Envelope envelope) {
            DateTime targetTime = new DateTime(2019, 8, 22, 23, 30, 29, 968);
            int diff = envelope.OriginatingTime.CompareTo(targetTime);

            Console.WriteLine("Audio: " + envelope.OriginatingTime);

            return diff;
        }

        /// <summary>
        /// Play binary audio.
        /// </summary>
        public void PlayBinaryAudio() {
            pipeline = Pipeline.Create();
            pipeline.PipelineCompleted += PipelineCompletedMethod;

            localStore = Store.Open(pipeline, storeName, storePath);
            audioSource = localStore.OpenStream<AudioBuffer>("Audio");

            audioSource.Out.Do((audio, envelope) => UpdateBuffer(audio, envelope));

            bufferedWaveProvider = new BufferedWaveProvider(new NAudio.Wave.WaveFormat(24000, 16, 2)) {
                DiscardOnBufferOverflow = true
            };

            waveOut = new WaveOut();
            waveOut.Init(bufferedWaveProvider);
            waveOut.Play();

            pipeline.RunAsync();
        }

        /// <summary>
        /// Update audio buffer.
        /// </summary>
        private void UpdateBuffer(AudioBuffer audio, Envelope envelope) {
            bufferedWaveProvider.AddSamples(audio.Data, 0, audio.Data.Length);
        }

        /// <summary>
        /// Name of the data store.
        /// </summary>
        private readonly string storeName;

        /// <summary>
        /// Path to the data store.
        /// </summary>
        private readonly string storePath;

        /// <summary>
        /// Pipeline.
        /// </summary>
        private Pipeline pipeline;

        /// <summary>
        /// Video device source.
        /// </summary>
        private IProducer<Shared<Image>> videoSource;

        /// <summary>
        /// Encoded image device source.
        /// </summary>
        private IProducer<Shared<EncodedImage>> encodedImageSource;

        /// <summary>
        /// Shared image device source.
        /// </summary>
        private IProducer<Shared<Image>> sharedImageSource;

        /// <summary>
        /// Audio device source.
        /// </summary>
        private IProducer<AudioBuffer> audioSource;

        /// <summary>
        /// Local store.
        /// </summary>
        private Importer localStore;

        /// <summary>
        /// Mpeg4 writer.
        /// </summary>
        private Mpeg4Writer mpeg4Writer;

        /// <summary>
        /// Wave writer.
        /// </summary>
        private Microsoft.Psi.Audio.WaveFileWriter waveWriter;

        /// <summary>
        /// NAudio wave out.
        /// </summary>
        private WaveOut waveOut;

        /// <summary>
        /// NAudio buffered wave provider.
        /// </summary>
        private BufferedWaveProvider bufferedWaveProvider;

        /// <summary>
        /// Pipeline completion method.
        /// </summary>
        private void PipelineCompletedMethod(object sender, PipelineCompletedEventArgs e) {
            if (e.Errors.Count > 0) {
                MessageBox.Show("Pipeline Completion Error: " + e.Errors[0].Message);
            }

            pipeline.PipelineCompleted -= PipelineCompletedMethod;

            waveOut?.Dispose();

            mpeg4Writer?.Dispose();
            waveWriter?.Dispose();

            pipeline?.Dispose();

            Console.WriteLine("DONE");
        }
    }
}
