using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg;

[Export(typeof(IComponentMetadata))]
public sealed class FrameToImageConverterMetadata : ConventionalComponentMetadata {

    public override string Name => "FFMpeg Frame to Image Converter";

    public override string Description => "[Experimental] Convert FFMpeg frames to \\psi images. Supports pixel format conversion and scaling.";

    protected override Type ComponentType => typeof(FrameToImageConverter);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(FrameToImageConverter.In) => "[Required] The input FFMpeg frames.",
            nameof(FrameToImageConverter.Out) => "The converted \\psi images.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new FrameToImageConverterConfiguration();
}
