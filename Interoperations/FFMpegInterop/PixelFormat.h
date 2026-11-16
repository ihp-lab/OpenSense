#pragma once

extern "C" {
#include <libavutil/pixfmt.h>
}

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for pixel formats that maps to FFmpeg's AVPixelFormat (int pixfmt.h)
    /// </summary>
    public enum class PixelFormat : int {
        None = AV_PIX_FMT_NONE,
        YUV420P = AV_PIX_FMT_YUV420P,
        RGB24 = AV_PIX_FMT_RGB24,
        BGR24 = AV_PIX_FMT_BGR24,
        YUV422P = AV_PIX_FMT_YUV422P,
        YUV444P = AV_PIX_FMT_YUV444P,
        YUV410P = AV_PIX_FMT_YUV410P,
        YUV411P = AV_PIX_FMT_YUV411P,
        Gray8 = AV_PIX_FMT_GRAY8,
        BGRA = AV_PIX_FMT_BGRA,
        Gray16LE = AV_PIX_FMT_GRAY16LE,
        BGR0 = AV_PIX_FMT_BGR0,
        RGBA64LE = AV_PIX_FMT_RGBA64LE,
    };
}
