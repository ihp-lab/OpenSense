using System;
using Microsoft.Psi.Imaging;
using OpenCvSharp;

namespace OpenSense.Components.OpenCvSharp {
    public static class ImageExtensions {

        public static Mat ToMat(this Image image) {
            if (image.PixelFormat != PixelFormat.BGR_24bpp) {
                throw new NotSupportedException("Only BGR 24bpp image is supported.");
            }
            var matType = MatType.CV_8UC3;
            var result = new Mat(image.Height, image.Width, matType, image.ImageData, image.Stride);
            return result;
        }
    }
}
