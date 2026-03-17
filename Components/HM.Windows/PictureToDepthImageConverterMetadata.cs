using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class PictureToDepthImageConverterMetadata : ConventionalComponentMetadata {

        public override string Name => "HM Picture to Depth Image Converter";

        public override string Description => "[Experimental] Convert HM Picture to \\psi DepthImage. Supports bit depth mapping and configurable depth metadata.";

        protected override Type ComponentType => typeof(PictureToDepthImageConverter);

        protected override string? GetPortDescription(string portName) => portName switch {
            nameof(PictureToDepthImageConverter.In) => "[Required] The input Picture stream.",
            nameof(PictureToDepthImageConverter.Out) => "The converted \\psi DepthImage stream with configured depth metadata.",
            _ => null,
        };

        public override ComponentConfiguration CreateConfiguration() => new PictureToDepthImageConverterConfiguration();
    }
}
