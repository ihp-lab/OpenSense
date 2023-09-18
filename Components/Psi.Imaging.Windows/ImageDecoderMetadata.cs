using System;
using System.Composition;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class ImageDecoderMetadata : ConventionalComponentMetadata {

        public override string Description => "Decodes encoded images to bitmap images. A Windows default implementation.";

        protected override Type ComponentType => typeof(ImageDecoder);

        public override string Name => "Image Decoder";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ImageDecoder.In):
                    return "[Required] Encoded images.";
                case nameof(ImageDecoder.Out):
                    return "Decoded bitmap images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ImageDecoderConfiguration();
    }
}
