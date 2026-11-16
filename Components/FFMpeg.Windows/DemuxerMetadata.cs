using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg;

[Export(typeof(IComponentMetadata))]
public sealed class DemuxerMetadata : ConventionalComponentMetadata {

    public override string Name => "FFMpeg Demuxer";

    public override string Description => "[Experimental] Read video and audio packets from a file using FFMpeg. The used FFMpeg is a regular and LGPL version and is dynamically linked.";

    protected override Type ComponentType => typeof(Demuxer);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(Demuxer.VideoOut) => "The output video packets. Index is the number of the video stream in the file, starting from 0.",
            nameof(Demuxer.AudioOut) => "The output audio packets. Index is the number of the audio stream in the file, starting from 0.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new DemuxerConfiguration();
}
