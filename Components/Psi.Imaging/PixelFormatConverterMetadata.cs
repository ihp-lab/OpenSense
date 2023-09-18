using System;
using System.Composition;

namespace OpenSense.Components.Psi.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class PixelFormatConverterMetadata : ConventionalComponentMetadata {

        public override string Description => "Converts the pixel format of images.";

        protected override Type ComponentType => typeof(PixelFormatConverter);

        public override string Name => "Pixel Format Converter";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(PixelFormatConverter.In):
                    return "[Required] Images need to be processed.";
                case nameof(PixelFormatConverter.Out):
                    return "Processed images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new PixelFormatConverterConfiguration();
    }
}
