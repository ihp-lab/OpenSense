using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Imaging.Visualizer.Common {
    public class ImageVisualizer : INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Settings

        #endregion

        #region Binding Fields
        private WriteableBitmap image = null;

        public WriteableBitmap Image {
            get => image;
            private set => SetProperty(ref image, value);
        }

        private float? frameRate = null;

        public float? FrameRate {
            get => frameRate;
            private set => SetProperty(ref frameRate, value);
        }
        #endregion

        #region API
        public void UpdateImage(Shared<Image> frame, DateTime originatingTime) {
            lock (imageSwapLock) {
                imageToRender?.Dispose();
                imageToRender = frame.AddRef();
            }

            UpdateFrame(originatingTime);
        }
        #endregion

        #region Bitmap Helpers
        private object imageSwapLock = new object();

        private Shared<Image> imageToRender = null;

        /// <summary>
        /// Register this call back on UI thread's dispatcher
        /// </summary>
        public void RenderingCallback(object sender, EventArgs args) {//Modification of canvas should be on UI thread, otherwise will get an exception
            lock (imageSwapLock) {
                if (imageToRender is null) {
                    return;
                }
                try {
                    if (Image is null) {
                        Image = CreateEmptyBitmap(imageToRender.Resource);
                    }
                    WriteToBitmap(imageToRender.Resource, Image);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                } finally {
                    imageToRender.Dispose();
                    imageToRender = null;
                }
            }
        }

        private static System.Windows.Media.PixelFormat ConvertPixelFormat(PixelFormat pixelFormat) => pixelFormat switch {
            PixelFormat.Gray_8bpp => System.Windows.Media.PixelFormats.Gray8,
            PixelFormat.Gray_16bpp => System.Windows.Media.PixelFormats.Gray16,
            PixelFormat.BGR_24bpp => System.Windows.Media.PixelFormats.Bgr24,
            PixelFormat.BGRX_32bpp => System.Windows.Media.PixelFormats.Bgr32,
            PixelFormat.BGRA_32bpp => System.Windows.Media.PixelFormats.Bgra32,
            _ => throw new InvalidOperationException("Unsupported /psi pixel format"),
        };

        private static WriteableBitmap CreateEmptyBitmap(Image image) {
            var pixelFormat = ConvertPixelFormat(image.PixelFormat);
            var width = image.Width;
            var height = image.Height;
            var result = new WriteableBitmap(width, height, dpiX: 96, dpiY: 96, pixelFormat, palette: null);
            return result;
        }

        private static void WriteToBitmap(Image source, WriteableBitmap destination) {
            if (destination.PixelWidth != source.Width || destination.PixelHeight != source.Height || destination.Format != ConvertPixelFormat(source.PixelFormat)) {
                throw new InvalidOperationException("Frame size/format in-consistant");
            }
            var width = source.Width;
            var height = source.Height;
            var rect = new System.Windows.Int32Rect(x: 0, y: 0, width: width, height: height);
            var bufferPtr = source.ImageData;
            var bufferSize = source.Size;
            var stride = source.Stride;
            destination.WritePixels(rect, bufferPtr, bufferSize, stride);//Lock() and Unlock() not needed for WritePixels()
        }
        #endregion

        #region FrameRate Helpers
        private const int frameBufferSize = 60;
        private DateTime[] frames = new DateTime[frameBufferSize];
        private long count = 0;

        private void UpdateFrame(DateTime time) {
            if (count < frameBufferSize) {
                count++;
                return;
            }
            var index = count % frameBufferSize;
            var otherTime = frames[index];
            frames[index] = time;
            count++;
            var diff = time - otherTime;
            var rate = frameBufferSize / diff.TotalSeconds;
            FrameRate = (float)rate;
        }
        #endregion
    }
}
