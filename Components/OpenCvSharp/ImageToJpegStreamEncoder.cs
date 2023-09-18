using System.IO;
using Microsoft.Psi.Imaging;
using OpenCvSharp;

namespace OpenSense.Components.OpenCvSharp {
    public sealed class ImageToJpegStreamEncoder : IImageToStreamEncoder {

        /// <inheritdoc/>
        public string Description => $"Jpeg({Quality})";

        /// <summary>
        /// Gets or sets JPEG image quality (0-100).
        /// </summary>
        public int Quality { get; set; } = 100;

        public void EncodeToStream(Image image, Stream stream) {
            using var mat = image.ToMat();
            var param = new ImageEncodingParam(ImwriteFlags.JpegQuality, Quality);
            mat.WriteToStream(stream, ".jpg", param);
        }
    }
}
