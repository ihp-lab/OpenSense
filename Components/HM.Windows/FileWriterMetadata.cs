using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileWriterMetadata : ConventionalComponentMetadata {

        public override string Name => "HM MP4 File Writer";

        public override string Description => "[Experimental] Write video to an MP4 file using HM (HEVC Model) encoder. Input is a Picture stream containing YUV data and metadata.";

        protected override Type ComponentType => typeof(FileWriter);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(FileWriter.In) => "The input Picture stream. Supports any ChromaFormat and bit depth. Bit depth and chroma format are auto-detected from SPS metadata.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new FileWriterConfiguration();
    }
}
