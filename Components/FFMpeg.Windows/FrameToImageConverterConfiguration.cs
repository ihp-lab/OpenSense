using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg;

[Serializable]
public sealed class FrameToImageConverterConfiguration : ConventionalComponentConfiguration {

    #region Options
    private PixelFormat outputPixelFormat = PixelFormat.BGR_24bpp;

    public PixelFormat OutputPixelFormat {
        get => outputPixelFormat;
        set => SetProperty(ref outputPixelFormat, value);
    }

    private int outputWidth;

    public int OutputWidth {
        get => outputWidth;
        set => SetProperty(ref outputWidth, value);
    }

    private int outputHeight;

    public int OutputHeight {
        get => outputHeight;
        set => SetProperty(ref outputHeight, value);
    }
    #endregion

    public override IComponentMetadata GetMetadata() => new FrameToImageConverterMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FrameToImageConverter(pipeline) {
        OutputPixelFormat = OutputPixelFormat,
        OutputWidth = OutputWidth,
        OutputHeight = OutputHeight,
    };
}
