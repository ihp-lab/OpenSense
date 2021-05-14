using System;
using System.Composition;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class ImageEncoderMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that encodes an image using a specified Microsoft.Psi.Imaging.IImageToStreamEncoder (Windows defaul implementation).";

        protected override Type ComponentType => typeof(ImageEncoder);

        public override ComponentConfiguration CreateConfiguration() => new ImageEncoderConfiguration();
    }
}
