#pragma once

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for pixel formats that maps to FFmpeg's AVPixelFormat
    /// </summary>
    public enum class PixelFormat : int {
        None = -1,      // AV_PIX_FMT_NONE
        RGB24 = 2,      // AV_PIX_FMT_RGB24
        BGR24 = 3,      // AV_PIX_FMT_BGR24
        YUV422P = 4,     //AV_PIX_FMT_YUV422P
        BGRA = 28,      // AV_PIX_FMT_BGRA
        Gray16LE = 30,  // AV_PIX_FMT_GRAY16LE
    };

    /// <summary>
    /// Utility class for PixelFormat operations
    /// </summary>
    public ref class PixelFormatHelper abstract sealed {
    public:
        /// <summary>
        /// Check if the pixel format is supported by the FFmpeg interop layer
        /// </summary>
        /// <param name="format">The pixel format to check</param>
        /// <returns>True if the format is supported, false otherwise</returns>
        static bool IsSupported(PixelFormat format) {
            switch (format) {
            case PixelFormat::RGB24:
            case PixelFormat::BGR24:
            case PixelFormat::YUV422P:
            case PixelFormat::BGRA:
            case PixelFormat::Gray16LE:
                return true;
            default:
                return false;
            }
        }
    };
} 