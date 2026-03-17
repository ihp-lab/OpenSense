using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class Mp4MuxerMetadata : ConventionalComponentMetadata {

        public override string Name => "HM MP4 Muxer";

        public override string Description => "[Experimental] Write HEVC Access Units to an MP4 file using minimp4 muxer.";

        protected override Type ComponentType => typeof(Mp4Muxer);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(Mp4Muxer.In) => "[Required] HEVC Access Unit stream. Must use a non-dropping DeliveryPolicy; dropping frames causes file corruption.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new Mp4MuxerConfiguration();
    }
}
