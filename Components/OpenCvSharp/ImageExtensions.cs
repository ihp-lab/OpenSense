using System;
using Microsoft.Psi.Imaging;
using OpenCvSharp;

namespace OpenSense.Components.OpenCvSharp {
    public static class ImageExtensions {

        /// <summary>
        /// Wrap an image to OpenCV Mat object.
        /// </summary>
        /// <remarks>
        /// OpenCV assumes that images are stored in BGR color space.
        /// This method does not do any conversion.
        /// Please be careful when using the returned Mat object.
        /// </remarks>
        public static Mat ToMat(this Image image) {
            Mat result;
            switch (image.PixelFormat) {
                case PixelFormat.BGR_24bpp:
                case PixelFormat.RGB_24bpp:
                    result = new Mat(image.Height, image.Width, MatType.CV_8UC3, image.ImageData, image.Stride);
                    break;
                case PixelFormat.BGRA_32bpp:
                    result = new Mat(image.Height, image.Width, MatType.CV_8UC4, image.ImageData, image.Stride);
                    break;
                case PixelFormat.Gray_8bpp:
                    result = new Mat(image.Height, image.Width, MatType.CV_8UC1, image.ImageData, image.Stride);
                    break;
                case PixelFormat.Gray_16bpp:
                    result = new Mat(image.Height, image.Width, MatType.CV_16UC1, image.ImageData, image.Stride);
                    break;
                default:
                    throw new NotSupportedException($"Pixel format {image.PixelFormat} is not supported.");
            }
            return result;
        }
    }
}
