namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// The unit of the position and size of a region.
/// </summary>
public enum RegionUnit {
    /// <summary>
    /// Pixels of the image.
    /// </summary>
    Pixel,

    /// <summary>
    /// A fraction of the size of the image: of its width for horizontal values, of its height for vertical values.
    /// </summary>
    Fraction,
}
