using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileReaderMetadata : ConventionalComponentMetadata {

        public override string Name => "HM MP4 File Reader";

        public override string Description => "[Experimental] Read frames from an MP4 file containing HEVC video using HM (HEVC Model) decoder.";

        protected override Type ComponentType => typeof(FileReader);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(FileReader.Out) => "The decoded PictureSnapshot stream containing raw pixel data and metadata (SPS, POC). No conversion or mapping is applied.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new FileReaderConfiguration();
    }
}
