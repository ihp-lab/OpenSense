namespace OpenSense.Components.Psi.Imaging;

/// <summary>
/// The point of a region that its position refers to. Positions are measured from the top-left corner of the image.
/// </summary>
public enum RegionAnchor {
    TopLeft,

    /// <summary>
    /// For an even size the center lies between two pixels, the position is the pixel after it.
    /// </summary>
    Center,
}
