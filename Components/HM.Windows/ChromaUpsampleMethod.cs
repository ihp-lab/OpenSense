namespace OpenSense.Components.HM {
    /// <summary>
    /// Algorithm used for chroma upsampling (e.g. 420 to 444).
    /// </summary>
    public enum ChromaUpsampleMethod {
        /// <summary>
        /// Duplicate nearest sample. Fastest, lowest quality.
        /// </summary>
        NearestNeighbor,

        /// <summary>
        /// Bilinear interpolation of adjacent samples. Moderate speed, better quality.
        /// </summary>
        Bilinear,
    }
}
