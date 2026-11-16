namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// When camera intrinsics of the output images are produced.
/// </summary>
public enum IntrinsicsOutput {
    /// <summary>
    /// For every output image, with the same originating time.
    /// </summary>
    EveryImage,

    /// <summary>
    /// Only when they differ from the previously produced ones.
    /// </summary>
    OnChange,
}
