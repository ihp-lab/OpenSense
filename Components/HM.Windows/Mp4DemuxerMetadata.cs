using System;
using System.Composition;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class Mp4DemuxerMetadata : ConventionalComponentMetadata {

        public override string Name => "HM MP4 Demuxer";

        public override string Description => "[Experimental] Read HEVC Access Units from an MP4 file using minimp4 demuxer.";

        protected override Type ComponentType => typeof(Mp4Demuxer);

        protected override string? GetPortDescription(string portName) {
            return portName switch {
                nameof(Mp4Demuxer.Out) => "HEVC Access Unit stream. Each message contains one frame's NAL units with timing metadata.",
                _ => null,
            };
        }

        public override ComponentConfiguration CreateConfiguration() => new Mp4DemuxerConfiguration();
    }
}
