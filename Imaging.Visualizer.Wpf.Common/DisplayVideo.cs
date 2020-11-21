using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Component.Imaging.Visualizer.Common {
    /// <summary>
    /// DisplayVideo class.
    /// </summary>
    public class DisplayVideo : INotifyPropertyChanged {
        private Shared<Image> psiImage;
        private Image psiDecodedImage;
        private WriteableBitmap bmpImage;

        private FrameCounter renderedFrames;
        private FrameCounter receivedFrames;

        private readonly object bmpLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayVideo"/> class.
        /// </summary>
        public DisplayVideo() : base() {
            renderedFrames = new FrameCounter();
            receivedFrames = new FrameCounter();

            bmpLock = new object();

            CompositionTarget.Rendering += UpdatePixels;
        }

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets.
        /// </summary>
        public FrameCounter RenderedFrames {
            get => renderedFrames;

            set {
                renderedFrames = value;
                OnPropertyChanged(nameof(RenderedFrames));
            }
        }

        /// <summary>
        /// Gets or sets.
        /// </summary>
        public FrameCounter ReceivedFrames {
            get => receivedFrames;

            set {
                receivedFrames = value;
                OnPropertyChanged(nameof(ReceivedFrames));
            }
        }

        /// <summary>
        /// Gets or sets.
        /// </summary>
        public WriteableBitmap VideoImage {
            get => bmpImage;

            set {
                bmpImage = value;
                OnPropertyChanged(nameof(VideoImage));
            }
        }

        /// <summary>
        /// Image update method.
        /// </summary>
        public void Update(Shared<Image> image) {
            lock (bmpLock) {
                receivedFrames.Increment();

                psiImage?.Dispose();
                psiImage = image.AddRef();
            }
        }

        /// <summary>
        /// Image update method.
        /// </summary>
        public void Update(Image image) {
            lock (bmpLock) {
                receivedFrames.Increment();

                //psiDecodedImage?.Dispose();
                psiDecodedImage = image.DeepClone();
            }
        }

        /// <summary>
        /// Image clear method.
        /// </summary>
        public void Clear() {
            lock (bmpLock) {
                if (psiImage != null) {
                    Shared<Image> tmpImage = psiImage.DeepClone();
                    tmpImage.Resource.Clear(System.Drawing.Color.White);

                    psiImage.Dispose();
                    psiImage = tmpImage.AddRef();
                }

                if (psiDecodedImage != null) {
                    Image tmpImage = psiDecodedImage.DeepClone();
                    tmpImage.Clear(System.Drawing.Color.White);

                    //psiDecodedImage.Dispose();
                    psiDecodedImage = tmpImage.DeepClone();
                }
            }
        }

        /// <summary>
        /// Update pixel values in the bmp image.
        /// </summary>
        private void UpdatePixels(object sender, EventArgs e) {
            lock (bmpLock) {
                if (psiImage != null && psiImage.Resource != null) {
                    if (VideoImage == null || VideoImage.PixelWidth != psiImage.Resource.Width || VideoImage.PixelHeight != psiImage.Resource.Height || VideoImage.BackBufferStride != psiImage.Resource.Stride) {
                        System.Windows.Media.PixelFormat pixelFormat;

                        switch (psiImage.Resource.PixelFormat) {
                            case Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp:
                                pixelFormat = PixelFormats.Gray8;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp:
                                pixelFormat = PixelFormats.Gray16;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp:
                                pixelFormat = PixelFormats.Bgr24;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGRX_32bpp:
                                pixelFormat = PixelFormats.Bgr32;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp:
                                pixelFormat = PixelFormats.Bgra32;
                                break;
                            default:
                                throw new Exception("Unexpected PixelFormat");
                        }

                        VideoImage = new WriteableBitmap(psiImage.Resource.Width, psiImage.Resource.Height, 300, 300, pixelFormat, null);
                    }

                    VideoImage.WritePixels(new Int32Rect(0, 0, psiImage.Resource.Width, psiImage.Resource.Height), psiImage.Resource.ImageData, psiImage.Resource.Stride * psiImage.Resource.Height, psiImage.Resource.Stride);

                    renderedFrames.Increment();
                }

                if (psiDecodedImage != null && psiDecodedImage.ImageData != null) {
                    if (VideoImage == null || VideoImage.PixelWidth != psiDecodedImage.Width || VideoImage.PixelHeight != psiDecodedImage.Height || VideoImage.BackBufferStride != psiDecodedImage.Stride) {
                        System.Windows.Media.PixelFormat pixelFormat;

                        switch (psiDecodedImage.PixelFormat) {
                            case Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp:
                                pixelFormat = PixelFormats.Gray8;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp:
                                pixelFormat = PixelFormats.Gray16;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp:
                                pixelFormat = PixelFormats.Bgr24;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGRX_32bpp:
                                pixelFormat = PixelFormats.Bgr32;
                                break;

                            case Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp:
                                pixelFormat = PixelFormats.Bgra32;
                                break;
                            default:
                                throw new Exception("Unexpected PixelFormat");
                        }

                        VideoImage = new WriteableBitmap(psiDecodedImage.Width, psiDecodedImage.Height, 300, 300, pixelFormat, null);
                    }

                    VideoImage.WritePixels(new Int32Rect(0, 0, psiDecodedImage.Width, psiDecodedImage.Height), psiDecodedImage.ImageData, psiDecodedImage.Stride * psiDecodedImage.Height, psiDecodedImage.Stride);

                    renderedFrames.Increment();
                }
            }
        }

        /// <summary>
        /// Fire an event when the image property changes.
        /// </summary>
        private void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
