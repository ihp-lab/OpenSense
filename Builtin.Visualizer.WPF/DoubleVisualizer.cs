using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Component.Builtin.Visualizer {
    public sealed class DoubleVisualizer : IConsumerProducer<double?, string>, INotifyPropertyChanged {

        #region Settings
        private string nullText = "?";

        public string NullText {
            get => nullText;
            set => SetProperty(ref nullText, value);
        }

        private string formatString = "F2";

        public string FormatString {
            get => formatString;
            set => SetProperty(ref formatString, value);
        }

        private bool autoClamp = true;

        public bool AutoClamp {
            get => autoClamp;
            set {
                var old = autoClamp;
                SetProperty(ref autoClamp, value);
                if (autoClamp != old) {
                    Reset();
                }
            }
        }

        private bool globalClamp = false;

        public bool GlobalClamp {
            get => globalClamp;
            set {
                var old = globalClamp;
                SetProperty(ref globalClamp, value);
                if (globalClamp != old) {
                    Reset();
                }
            }
        }

        private double clampHighValue = 1;

        public double ClampHighValue {
            get => clampHighValue;
            set {
                var old = clampHighValue;
                SetProperty(ref clampHighValue, value);
                if (clampHighValue != old) {
                    Reset();
                }
            }
        }

        private double clampLowValue = 0;

        public double ClampLowValue {
            get => clampLowValue;
            set {
                var old = clampLowValue;
                SetProperty(ref clampLowValue, value);
                if (clampLowValue != old) {
                    Reset();
                }
            }
        }

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

        private int numSamplesPerPixel = 1;

        public int NumSamplesPerPixel {
            get => numSamplesPerPixel;
            set {
                if (value <= 0) {
                    throw new ArgumentOutOfRangeException(nameof(NumSamplesPerPixel));
                }
                var old = numSamplesPerPixel;
                SetProperty(ref numSamplesPerPixel, value);
                if (numSamplesPerPixel != old) {
                    Reset();
                }
            }
        }
        #endregion

        #region Ports
        public Receiver<double?> In { get; } 

        public Emitter<string> Out { get; }
        #endregion

        private double accumulatedValue;

        private int accumulatedSampleCount;

        public DoubleVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<double?>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(double? value, Envelope envelope) {
            Value = value;
            var text = value is null ? NullText : value.Value.ToString(FormatString);
            Text = text;
            Out.Post(text, envelope.OriginatingTime);

            lock (lockObj) {
                if (drawingSampleBuffer is null) {
                    drawingSampleBuffer_Length = ImageWidth;
                    drawingSampleBuffer = new double[drawingSampleBuffer_Length];

                    accumulatedValue = 0;
                    accumulatedSampleCount = 0;

                    MaxDrawingValue = double.NegativeInfinity;
                    MinDrawingValue = double.PositiveInfinity;
                }

                if (value is not null && !double.IsNaN(value.Value)) {
                    accumulatedValue += val.Value;
                    accumulatedSampleCount++;
                    if (accumulatedSampleCount >= NumSamplesPerPixel) {
                        var index = drawingSampleBuffer_Count % drawingSampleBuffer_Length;
                        var mean = accumulatedValue / accumulatedSampleCount;
                        var val = mean;
                        if (!double.IsNaN(val) && !double.IsInfinity(val)) {
                            drawingSampleBuffer[index] = val;
                            if (AutoClamp && GlobalClamp) {
                                MaxDrawingValue = Math.Max(MaxDrawingValue, val);
                                MinDrawingValue = Math.Min(MinDrawingValue, val);
                            }
                            drawingSampleBuffer_Count++;

                            accumulatedValue = 0;
                            accumulatedSampleCount = 0;
                        }

                        imageBackingDataUpdated = true;
                    }
                }
            }
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Value = null;
            Text = null;
        }

        #region Data Bindings
        private double? val;

        public double? Value {
            get => val;
            private set => SetProperty(ref val, value);
        }

        private string text;

        public string Text {
            get => text;
            private set => SetProperty(ref text, value);
        }

        private double maxDrawingValue;

        public double MaxDrawingValue {
            get => maxDrawingValue;
            private set => SetProperty(ref maxDrawingValue, value);
        }

        private double minDrawingValue;

        public double MinDrawingValue {
            get => minDrawingValue;
            private set => SetProperty(ref minDrawingValue, value);
        }

        private WriteableBitmap image;

        public WriteableBitmap Image {
            get => image;
        }
        #endregion

        private void Reset() {
            lock (lockObj) {
                Volatile.Write(ref drawingSampleBuffer, null);
                Volatile.Write(ref image, null);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
            }
        }

        #region Drawing

        private object lockObj = new object();

        private bool imageBackingDataUpdated = false;

        private double[] drawingSampleBuffer = null;

        private int drawingSampleBuffer_Length;

        private long drawingSampleBuffer_Count = 0;

        private const byte foregroundColor = byte.MinValue;

        private const byte backgroundColor = byte.MaxValue;

        private byte[] backgroundStrip;

        private byte[] foregroundStrip;

        /// <summary>
        /// Register this call back on UI thread's dispatcher
        /// </summary>
        public void RenderingCallback(object sender, EventArgs args) {
            lock (lockObj) {
                if (drawingSampleBuffer is null || !imageBackingDataUpdated) {
                    return;
                }

                int pixelWidth;
                int pixelHeight;
                if (Image is null) {
                    var img = new WriteableBitmap(ImageWidth, ImageHeight, dpiX: 96, dpiY: 96, System.Windows.Media.PixelFormats.Gray8, palette: null);
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

                    //TODO: draw marks here

                    image = img;
                }

                ClearImage();
                pixelWidth = image.PixelWidth;
                pixelHeight = image.PixelHeight;

                double localMaxDrawingValue;
                double localMinDrawingValue;
                if (AutoClamp) {
                    if (!GlobalClamp) {
                        localMaxDrawingValue = double.NegativeInfinity;
                        localMinDrawingValue = double.PositiveInfinity;
                        for (var i = 0; i < pixelWidth; i++) {
                            var index = (drawingSampleBuffer_Count + drawingSampleBuffer_Length - i) % drawingSampleBuffer_Length;
                            var val = drawingSampleBuffer[index];
                            localMaxDrawingValue = Math.Max(localMaxDrawingValue, val);
                            localMinDrawingValue = Math.Min(localMinDrawingValue, val);
                        }
                    } else {
                        localMaxDrawingValue = MaxDrawingValue;
                        localMinDrawingValue = MinDrawingValue;
                    } 
                } else {
                    localMaxDrawingValue = ClampHighValue;
                    localMinDrawingValue = ClampLowValue;
                }

                var minMaxDiff = localMaxDrawingValue - localMinDrawingValue;
                if (minMaxDiff > 0 && !double.IsPositiveInfinity(minMaxDiff)) {
                    for (var i = 0; i < pixelWidth; i++) {
                        var x = (pixelWidth - 1) - i;
                        var index = (drawingSampleBuffer_Count + drawingSampleBuffer_Length - i) % drawingSampleBuffer_Length;
                        var val = drawingSampleBuffer[index];
                        var percentage = (val - localMinDrawingValue) / minMaxDiff;
                        if (0 <= percentage && percentage <= 1) {
                            var rawY = Math.Round((pixelHeight - 1) * (1 - percentage));
                            var y = Math.Max(0, Math.Min(pixelHeight - 1, (int)rawY));
                            var updateRect = new System.Windows.Int32Rect(x: x, y: y, width: 1, height: 1);
                            Image.WritePixels(updateRect, foregroundStrip, stride: 1, offset: 0);
                        }
                    }

                    imageBackingDataUpdated = false;

                    MaxDrawingValue = localMaxDrawingValue;
                    MinDrawingValue = localMinDrawingValue;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
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
