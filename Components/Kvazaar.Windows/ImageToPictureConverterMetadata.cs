using System;
using System.Composition;

namespace OpenSense.Components.Kvazaar {
    [Export(typeof(IComponentMetadata))]
    public sealed class ImageToPictureConverterMetadata : ConventionalComponentMetadata {

        public override string Name => "Kvazaar Image to Picture Converter";

        public override string Description => "[Experimental] Convert \\psi Image to Kvazaar Picture."
#if FIXED_BIT_DEPTH
            + $" Due to Kvazaar limitations, this build only supports {KvazaarInterop.Picture.MaxBitDepth}-bit output."
#endif
            ;

        protected override Type ComponentType => typeof(ImageToPictureConverter);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(ImageToPictureConverter.In) => "[Required] The input \\psi Image stream.",
                nameof(ImageToPictureConverter.Out) => "The converted Kvazaar Picture stream.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new ImageToPictureConverterConfiguration();
    }
}
