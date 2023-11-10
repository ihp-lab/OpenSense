using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.OpenCvSharp {
    public static class SharedImageExtensions {

        /// <summary>
        /// Encodes an image to a JPEG stream.
        /// </summary>
        public static IProducer<Shared<EncodedImage>> EncodeJpeg(
            this IProducer<Shared<Image>> image, 
            int quality = 90,
            DeliveryPolicy<Shared<Image>>? deliveryPolicy = null,
            string name = "Encode"
            ) {
            var encoder = new ImageToJpegStreamEncoder() { 
                Quality = quality,
            };
            var result = image.Encode(encoder, deliveryPolicy, name);
            return result;
        }
    }
}
