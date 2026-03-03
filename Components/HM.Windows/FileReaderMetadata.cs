using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileReaderMetadata : ConventionalComponentMetadata {

        public override string Name => "HM HEVC File Reader";

        public override string Description => "[Experimental] Read frames from an MP4 file containing HEVC video using HM (HEVC Model) decoder.";

        protected override Type ComponentType => typeof(FileReader);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FileReader.Out):
                    return "The decoded image stream in \\psi format. Only available for Chroma400 content, because \\psi's PixelFormat has no multi-channel 16-bit format.";
                case nameof(FileReader.PictureOut):
                    return "The decoded HM PicYuv stream. Always available regardless of chroma format.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FileReaderConfiguration();
    }
}
