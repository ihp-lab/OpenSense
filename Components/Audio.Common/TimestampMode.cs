namespace OpenSense.Components.Audio {
    public enum TimestampMode {
        Unspecified = 0,

        AtStart = 1,

        /// <summary>
        /// Audio buffers are timestamped at the end of the buffer. This is the default in Microsoft \psi.
        /// </summary>
        AtEnd = 2,
    }
}
