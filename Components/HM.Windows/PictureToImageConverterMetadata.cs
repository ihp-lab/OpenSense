using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class PictureToImageConverterMetadata : ConventionalComponentMetadata {

        public override string Name => "HM Picture to Image Converter";

        public override string Description => "[Experimental] Convert HM Picture to \\psi Image. Supports chroma conversion, bit depth mapping, and multiple output pixel formats.";

        protected override Type ComponentType => typeof(PictureToImageConverter);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(PictureToImageConverter.In) => "[Required] The input Picture stream.",
                nameof(PictureToImageConverter.Out) => "The converted \\psi Image stream. Supports Gray_8bpp, Gray_16bpp, BGR_24bpp, RGB_24bpp, and RGBA_64bpp via OutputPixelFormat setting.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new PictureToImageConverterConfiguration();
    }
}
