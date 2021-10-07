using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Component.Audio.Visualizer {
    public class AudioVisualizer : Subpipeline, IConsumer<AudioBuffer>, INotifyPropertyChanged {

        #region Settings
        private int imageWidth = 1280;

        public int ImageWidth {
            get => imageWidth;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(ImageWidth));
                }
                var old = imageWidth;
                SetProperty(ref imageWidth, value);
                if (imageWidth != old) {
                    Reset();
                }
            }
        }

        private int imageHeight = 720;

        public int ImageHeight {
            get => imageHeight;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(ImageHeight));
                }
                var old = imageHeight;
                SetProperty(ref imageHeight, value);
                if (imageHeight != old) {
                    Reset();
                }
            }
        }

        private TimeSpan duration = TimeSpan.FromSeconds(10);

        public TimeSpan Duration {
            get => duration;
            set {
                if (value.Ticks <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(Duration));
                }
                var old = duration;
                SetProperty(ref duration, value);
                if (duration != old) {
                    Reset();
                }
            }
        }
        #endregion

        #region Ports

        public Receiver<AudioBuffer> In { get; private set; }
        #endregion

        #region Data Bindings

        private WriteableBitmap image;

        public WriteableBitmap Image {
            get => image;
        }
        #endregion

        public AudioVisualizer(Pipeline pipeline) : base(pipeline) {
            In = pipeline.CreateReceiver<AudioBuffer>(pipeline, Process, nameof(In));
        }

        private int numSamplesForEachPixel;

        private float[] accumulateBuffer;

        private int accumulatedSampleCount;

        private void Process(AudioBuffer buffer, Envelope envelope) {
            lock (lockObj) {
                if (drawingSampleBuffer is null) {
                    drawingSampleBuffer_Channels = buffer.Format.Channels;
                    drawingSampleBuffer_Length = ImageWidth;
                    drawingSampleBuffer = new float[drawingSampleBuffer_Length, drawingSampleBuffer_Channels];

                    accumulateBuffer = new float[drawingSampleBuffer_Channels];
                    accumulatedSampleCount = 0;

                    var secondsEachPixel = Duration.TotalSeconds / ImageWidth;
                    numSamplesForEachPixel = (int)Math.Ceiling(buffer.Format.SamplesPerSec * secondsEachPixel);
                }

                Debug.Assert(buffer.Format.ExtraSize == 0);

                var loc = 0;
                while (loc < buffer.Data.Length) {
                    for (var i = 0; i < drawingSampleBuffer_Channels; i++) {
                        var sample = ReadOneSample(buffer, ref loc);
                        accumulateBuffer[i] += Math.Abs(sample);
                    }
                    accumulatedSampleCount++;
                    if (accumulatedSampleCount >= numSamplesForEachPixel) {
                        var index = drawingSampleBuffer_Count % drawingSampleBuffer_Length;
                        for (var i = 0; i < drawingSampleBuffer_Channels; i++) {
                            var mean = accumulateBuffer[i] / accumulatedSampleCount;
                            var val = Math.Max(0, Math.Min(1, mean));
                            drawingSampleBuffer[index, i] = val;
                        }
                        drawingSampleBuffer_Count++;

                        Array.Clear(accumulateBuffer, 0, accumulateBuffer.Length);
                        accumulatedSampleCount = 0;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ReadOneSample(AudioBuffer buffer, ref int location) {
            float result;
            switch (buffer.Format.FormatTag) {
                case WaveFormatTag.WAVE_FORMAT_IEEE_FLOAT:
                    Debug.Assert(buffer.Format.BitsPerSample == 8 * sizeof(float));
                    result = BitConverter.ToSingle(buffer.Data, location);
                    Debug.Assert(-1 <= result && result <= 1);
                    location += sizeof(float);
                    return result;
                case WaveFormatTag.WAVE_FORMAT_PCM:
                    switch (buffer.Format.BitsPerSample) {
                        case 8:
                            result = (float)buffer.Data[location] / byte.MaxValue;
                            location += sizeof(byte);
                            return result;
                        case 16://signed
                            var s = BitConverter.ToInt16(buffer.Data, location);
                            result = (float)s / short.MaxValue;
                            location += sizeof(ushort);
                            return result;
                        default:
                            throw new NotSupportedException($"Audio visualizer does not support calculating audio with {buffer.Format.BitsPerSample} per sample");
                    }
                default:
                    throw new NotSupportedException($"Audio visualizer does not support calculating audio with {buffer.Format.FormatTag} fromat data");
            }
        }

        private void Reset() {
            lock (lockObj) {
                Volatile.Write(ref drawingSampleBuffer, null);
                Volatile.Write(ref image, null);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }

        #region Drawing

        private object lockObj = new object();

        private float[,] drawingSampleBuffer = null;

        private int drawingSampleBuffer_Channels;

        private int drawingSampleBuffer_Length;

        private long drawingSampleBuffer_Count = 0;

        private int heightEachChannel;

        private int halfHeightEachChannel;

        private int[] channelCenterHeight;

        private const byte foregroundColor = byte.MinValue;

        private const byte backgroundColor = byte.MaxValue;

        private byte[] backgroundStrip;

        private byte[] foregroundStrip;

        /// <summary>
        /// Register this call back on UI thread's dispatcher
        /// </summary>
        public void RenderingCallback(object sender, EventArgs args) {
            lock(lockObj) {
                if (drawingSampleBuffer is null) {
                    return;
                }

                int pixelWidth;
                int pixelHeight;
                if (Image is null) {
                    var img = new WriteableBitmap(ImageWidth, ImageHeight, dpiX:96, dpiY: 96, System.Windows.Media.PixelFormats.Gray8, palette: null);
                    pixelWidth = img.PixelWidth;
                    pixelHeight = img.PixelHeight;

                    //fill background
                    foregroundStrip = new byte[Math.Max(pixelWidth, pixelHeight)];
                    for (var i = 0; i < foregroundStrip.Length; i++) {
                        foregroundStrip[i] = foregroundColor;
                    }
                    backgroundStrip = new byte[Math.Max(pixelWidth, pixelHeight)];
                    for (var i = 0; i < backgroundStrip.Length; i++) {
                        backgroundStrip[i] = backgroundColor;
                    }

                    //draw vertical lines for channels
                    channelCenterHeight = new int[drawingSampleBuffer_Channels];
                    heightEachChannel = pixelHeight / drawingSampleBuffer_Channels;
                    halfHeightEachChannel = heightEachChannel / 2;
                    for (var i = 0; i < drawingSampleBuffer_Channels; i++) {
                        channelCenterHeight[i] = i * heightEachChannel + halfHeightEachChannel;
                    }

                    image = img;
                }

                ClearImage();

                pixelWidth = image.PixelWidth;
                pixelHeight = image.PixelHeight;
                for (var i = 0; i < pixelWidth; i++) {
                    var x = (pixelWidth - 1) - i;
                    var index = (drawingSampleBuffer_Count + drawingSampleBuffer_Length - i) % drawingSampleBuffer_Length;
                    for (var c = 0; c < drawingSampleBuffer_Channels; c++) {
                        var val = drawingSampleBuffer[index, c];
                        var halfBarHeight = (int)(halfHeightEachChannel * val);
                        var updateRect = new System.Windows.Int32Rect(x: x, y: channelCenterHeight[c] - halfBarHeight, width: 1, height: 2 * halfBarHeight);
                        Image.WritePixels(updateRect, foregroundStrip, stride: 1, offset: 0);
                    }
                }

                DrawOverlayLines();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }

        private void ClearImage() {
            var pixelWidth = Image.PixelWidth;
            var pixelHeight = Image.PixelHeight;
            for (var i = 0; i < pixelHeight; i++) {
                var updateRect = new System.Windows.Int32Rect(x: 0, y: i, width: pixelWidth, height: 1);
                Image.WritePixels(updateRect, backgroundStrip, stride: backgroundStrip.Length, offset: 0);
            }
        }

        private void DrawOverlayLines() {
            var pixelWidth = Image.PixelWidth;
            var pixelHeight = Image.PixelHeight;
            for (var i = 0; i < drawingSampleBuffer_Channels; i++) {
                var updateRect = new System.Windows.Int32Rect(x: 0, y: channelCenterHeight[i], width: pixelWidth, height: 1);
                Image.WritePixels(updateRect, foregroundStrip, stride: foregroundStrip.Length, offset: 0);
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        } 
        #endregion
    }
}
