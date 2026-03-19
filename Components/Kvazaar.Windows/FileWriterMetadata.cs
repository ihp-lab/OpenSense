using System;
using System.Composition;
using KvazaarInterop;

namespace OpenSense.Components.Kvazaar {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileWriterMetadata : ConventionalComponentMetadata {

        public override string Name => "Kvazaar MP4 File Writer";

        public override string Description => "[Experimental] Write video to an MP4 file using Kvazaar HEVC encoder."
#if FIXED_BIT_DEPTH
            + $" Due to Kvazaar limitations, this build only supports {Picture.MaxBitDepth}-bit encoding."
#endif
            ;

        protected override Type ComponentType => typeof(FileWriter);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(FileWriter.In) => "[Required] Kvazaar Picture stream. Supports any ChromaFormat and bit depth. Must use a non-dropping DeliveryPolicy; dropping frames causes file corruption.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new FileWriterConfiguration();
    }
}
