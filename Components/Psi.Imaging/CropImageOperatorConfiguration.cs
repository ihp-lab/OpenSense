#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Psi.Imaging;

[Serializable]
public sealed class CropImageOperatorConfiguration : ConventionalComponentConfiguration {

    #region Options
    private RegionAnchor anchor = RegionAnchor.TopLeft;

    public RegionAnchor Anchor {
        get => anchor;
        set => SetProperty(ref anchor, value);
    }

    private RegionUnit unit = RegionUnit.Pixel;

    public RegionUnit Unit {
        get => unit;
        set => SetProperty(ref unit, value);
    }

    private double x;

    public double X {
        get => x;
        set => SetProperty(ref x, value);
    }

    private double y;

    public double Y {
        get => y;
        set => SetProperty(ref y, value);
    }

    private double width = 640;

    public double Width {
        get => width;
        set => SetProperty(ref width, value);
    }

    private double height = 480;

    public double Height {
        get => height;
        set => SetProperty(ref height, value);
    }

    private RegionPadding padding = RegionPadding.Black;

    public RegionPadding Padding {
        get => padding;
        set => SetProperty(ref padding, value);
    }

    private IntrinsicsMatching intrinsicsMatching = IntrinsicsMatching.Latest;

    public IntrinsicsMatching IntrinsicsMatching {
        get => intrinsicsMatching;
        set => SetProperty(ref intrinsicsMatching, value);
    }

    private ImageWithoutIntrinsics imageWithoutIntrinsics = ImageWithoutIntrinsics.Drop;

    public ImageWithoutIntrinsics ImageWithoutIntrinsics {
        get => imageWithoutIntrinsics;
        set => SetProperty(ref imageWithoutIntrinsics, value);
    }

    private IntrinsicsOutput intrinsicsOutput = IntrinsicsOutput.EveryImage;

    public IntrinsicsOutput IntrinsicsOutput {
        get => intrinsicsOutput;
        set => SetProperty(ref intrinsicsOutput, value);
    }
    #endregion

    public override IComponentMetadata GetMetadata() => new CropImageOperatorMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new CropImageOperator(pipeline) {
        Anchor = Anchor,
        Unit = Unit,
        X = X,
        Y = Y,
        Width = Width,
        Height = Height,
        Padding = Padding,
        IntrinsicsMatching = IntrinsicsMatching,
        ImageWithoutIntrinsics = ImageWithoutIntrinsics,
        IntrinsicsOutput = IntrinsicsOutput,
    };
}
