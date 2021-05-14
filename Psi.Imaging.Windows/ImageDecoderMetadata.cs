using System;
using System.Composition;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class ImageDecoderMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that decodes an image using Microsoft.Psi.Imaging.ImageFromStreamDecoder (Windows defaul implementation).";

        protected override Type ComponentType => typeof(ImageDecoder);

        public override ComponentConfiguration CreateConfiguration() => new ImageDecoderConfiguration();
    }
}
