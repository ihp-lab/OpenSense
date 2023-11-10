using System;
using System.IO;
using Microsoft.Psi.Imaging;
using OpenCvSharp;

namespace OpenSense.Components.OpenCvSharp {
    public sealed class ImageToJpegStreamEncoder : IImageToStreamEncoder {

        /// <summary>
        /// Gets or sets JPEG image quality (0-100).
        /// </summary>
        public int Quality { get; set; } = 100;


        #region IImageToStreamEncoder
        /// <inheritdoc/>
        public string Description => $"Jpeg({Quality})";

        /// <inheritdoc/>
        public void EncodeToStream(Image image, Stream stream) {
            Mat mat;
            switch (image.PixelFormat) {
                case PixelFormat.BGR_24bpp:
                case PixelFormat.Gray_8bpp:
                case PixelFormat.Gray_16bpp://Not sure how this is processed
                    mat = image.ToMat();
                    break;
                case PixelFormat.RGB_24bpp: {
                        using var temp = image.ToMat();
                        mat = new Mat(image.Height, image.Width, MatType.CV_8UC3);
                        Cv2.CvtColor(temp, mat, ColorConversionCodes.RGB2BGR);
                    }
                    break;
                case PixelFormat.BGRA_32bpp: {
                        using var temp = image.ToMat();
                        mat = new Mat(image.Height, image.Width, MatType.CV_8UC3);
                        Cv2.CvtColor(temp, mat, ColorConversionCodes.BGRA2BGR);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Pixel format {image.PixelFormat} is not supported.");
            }
            try {
                var param = new ImageEncodingParam(ImwriteFlags.JpegQuality, Quality);
                mat.WriteToStream(stream, ".jpg", param);
            } finally {
                mat.Dispose();
            }
        } 
        #endregion
    }
}
