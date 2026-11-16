namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// How camera intrinsics are matched to images.
/// </summary>
public enum IntrinsicsMatching {
    /// <summary>
    /// The most recently received intrinsics.
    /// </summary>
    Latest,

    /// <summary>
    /// Only intrinsics with the same originating time as the image.
    /// </summary>
    Exact,
}
