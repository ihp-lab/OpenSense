#nullable enable

using System;
using System.Composition;

namespace OpenSense.Components.Psi.Imaging;

[Export(typeof(IComponentMetadata))]
public sealed class CropImageOperatorMetadata : ConventionalComponentMetadata {

    public override string Name => "Crop Image";

    public override string Description => "Crop images to a region. When camera intrinsics are given, intrinsics of the cropped images are also produced.";

    protected override Type ComponentType => typeof(CropImageOperator);

    protected override string? GetPortDescription(string portName) {
        return portName switch {
            nameof(CropImageOperator.In) => "[Required] The input images.",
            nameof(CropImageOperator.IntrinsicsIn) => "Camera intrinsics of the input images.",
            nameof(CropImageOperator.Out) => "The cropped images.",
            nameof(CropImageOperator.IntrinsicsOut) => "Camera intrinsics of the cropped images, with the same timestamps as the images they belong to. The principal point is moved by the position of the region.",
            _ => null,
        };
    }

    public override ComponentConfiguration CreateConfiguration() => new CropImageOperatorConfiguration();
}
