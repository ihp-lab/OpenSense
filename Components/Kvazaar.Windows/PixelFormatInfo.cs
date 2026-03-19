using System;
using KvazaarInterop;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    /// <summary>
    /// Derives bit depth and chroma format requirements from pixel format.
    /// Shared by components and WPF controls to avoid duplicating derivation logic.
    /// </summary>
    public static class PixelFormatInfo {

        /// <summary>
        /// Get the bit depth implied by the given pixel format.
        /// </summary>
        public static int GetBitDepth(PixelFormat pixelFormat) => pixelFormat switch {
            PixelFormat.Gray_8bpp or PixelFormat.BGR_24bpp or PixelFormat.RGB_24bpp => 8,
            PixelFormat.Gray_16bpp => 16,
            _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
        };

        /// <summary>
        /// Get the chroma format required by the given pixel format.
        /// </summary>
        public static ChromaFormat GetRequiredChromaFormat(PixelFormat pixelFormat) => pixelFormat switch {
            PixelFormat.Gray_8bpp or PixelFormat.Gray_16bpp => ChromaFormat.Csp400,
            PixelFormat.BGR_24bpp or PixelFormat.RGB_24bpp => ChromaFormat.Csp444,
            _ => throw new NotSupportedException($"Pixel format {pixelFormat} is not supported."),
        };
    }
}
