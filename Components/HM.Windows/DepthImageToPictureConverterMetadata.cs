using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class DepthImageToPictureConverterMetadata : ConventionalComponentMetadata {

        public override string Name => "HM Depth Image to Picture Converter";

        public override string Description => "[Experimental] Convert \\psi DepthImage to HM Picture. Validates depth metadata (semantics and scale factor) and supports bit depth mapping.";

        protected override Type ComponentType => typeof(DepthImageToPictureConverter);

        protected override string? GetPortDescription(string portName) => portName switch {
            nameof(DepthImageToPictureConverter.In) => "[Required] The input \\psi DepthImage stream.",
            nameof(DepthImageToPictureConverter.Out) => "The converted HM Picture stream (Chroma400, configurable bit depth).",
            _ => null,
        };

        public override ComponentConfiguration CreateConfiguration() => new DepthImageToPictureConverterConfiguration();
    }
}
