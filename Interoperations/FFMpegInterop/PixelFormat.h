#pragma once

using namespace System;

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for pixel formats that maps to FFmpeg's AVPixelFormat (int pixfmt.h)
    /// </summary>
    public enum class PixelFormat : int {
        None = -1,      // AV_PIX_FMT_NONE
        YUV420P = 0,   // AV_PIX_FMT_YUV420P
        RGB24 = 2,      // AV_PIX_FMT_RGB24
        BGR24 = 3,      // AV_PIX_FMT_BGR24
        YUV422P = 4,     //AV_PIX_FMT_YUV422P
        YUV444P = 5,     // AV_PIX_FMT_YUV444P
        YUV410P = 6,   // AV_PIX_FMT_YUV410P
        YUV411P = 7,   // AV_PIX_FMT_YUV411P
        GRAY8 = 8,      // AV_PIX_FMT_GRAY8
        BGRA = 28,      // AV_PIX_FMT_BGRA
        Gray16LE = 30,  // AV_PIX_FMT_GRAY16LE
    };

    /// <summary>
    /// Utility class for PixelFormat operations
    /// </summary>
    public ref class PixelFormatHelper abstract sealed {
    public:
        /// <summary>
        /// Check if the pixel format is defined
        /// </summary>
        /// <param name="format">The pixel format to check</param>
        /// <returns>True if the format is defined, false otherwise</returns>
        static bool IsDefined(PixelFormat format) {
            return Enum::IsDefined(PixelFormat::typeid, format);
        }

        /// <summary>
        /// Check if the pixel format is supported by the FFmpeg interop layer
        /// </summary>
        /// <param name="format">The pixel format to check</param>
        /// <returns>True if the format is supported, false otherwise</returns>
        static bool IsSupported(PixelFormat format) {
            return format != PixelFormat::None && IsDefined(format);
        }

        
    };
} 