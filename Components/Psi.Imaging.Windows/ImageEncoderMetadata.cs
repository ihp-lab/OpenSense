using System;
using System.Composition;
using Microsoft.Psi.Imaging;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class ImageEncoderMetadata : ConventionalComponentMetadata {

        public override string Description => "Encodes bitmap images to another format. A Windows default implementation.";

        protected override Type ComponentType => typeof(ImageEncoder);

        public override string Name => "Image Encoder";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ImageEncoder.In):
                    return "[Required] Bitmap images.";
                case nameof(ImageEncoder.Out):
                    return "Encoded images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ImageEncoderConfiguration();
    }
}
