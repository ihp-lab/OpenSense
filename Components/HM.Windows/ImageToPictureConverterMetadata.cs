using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class ImageToPictureConverterMetadata : ConventionalComponentMetadata {

        public override string Name => "HM Image to Picture Converter";

        public override string Description => "[Experimental] Convert \\psi Image to HM Picture. Intended for testing HM encoder pipeline with standard image sources.";

        protected override Type ComponentType => typeof(ImageToPictureConverter);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ImageToPictureConverter.In):
                    return "The input \\psi Image stream.";
                case nameof(ImageToPictureConverter.Out):
                    return "The converted HM Picture stream.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ImageToPictureConverterConfiguration();
    }
}
