#pragma once

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for pixel formats that maps to FFmpeg's AVPixelFormat
    /// </summary>
    public enum class PixelFormat : int {
        None = -1,      // AV_PIX_FMT_NONE
        RGB24 = 2,      // AV_PIX_FMT_RGB24
        BGR24 = 3,      // AV_PIX_FMT_BGR24
        Gray8 = 8,      // AV_PIX_FMT_GRAY8
        BGRA = 28,      // AV_PIX_FMT_BGRA
        Gray16LE = 30,  // AV_PIX_FMT_GRAY16LE
        RGBA64LE = 105, // AV_PIX_FMT_RGBA64LE
    };
} 