namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// What to do with an image that has no matching camera intrinsics.
/// </summary>
public enum ImageWithoutIntrinsics {
    Drop,
    Error,
}
