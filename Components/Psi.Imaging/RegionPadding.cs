namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// How the part of a region outside the image is filled.
/// </summary>
public enum RegionPadding {
    Black,

    /// <summary>
    /// Not filled, a region reaching outside the image is an error.
    /// </summary>
    Error,
}
