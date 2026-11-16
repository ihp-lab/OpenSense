using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg;

[Export(typeof(IComponentMetadata))]
public sealed class VideoDecoderMetadata : ConventionalComponentMetadata {

    public override string Name => "FFMpeg Video Decoder";

    public override string Description => "[Experimental] Decode video packets into frames using FFMpeg. The used FFMpeg is a regular and LGPL version and is dynamically linked.";

    protected override Type ComponentType => typeof(VideoDecoder);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(VideoDecoder.In) => "[Required] The input video packets. Must use a non-dropping DeliveryPolicy.",
            nameof(VideoDecoder.Out) => "The output FFMpeg frames.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new VideoDecoderConfiguration();
}
