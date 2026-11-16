using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg;

[Export(typeof(IComponentMetadata))]
public sealed class AudioDecoderMetadata : ConventionalComponentMetadata {

    public override string Name => "FFMpeg Audio Decoder";

    public override string Description => "[Experimental] Decode audio packets into audio signal using FFMpeg. The used FFMpeg is a regular and LGPL version and is dynamically linked.";

    protected override Type ComponentType => typeof(AudioDecoder);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(AudioDecoder.In) => "[Required] The input audio packets. Must use a non-dropping DeliveryPolicy.",
            nameof(AudioDecoder.Out) => "The output audio signal. Sample rate and channels are the same as in the file.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new AudioDecoderConfiguration();
}
