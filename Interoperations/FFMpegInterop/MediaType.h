#pragma once

extern "C" {
#include <libavutil/avutil.h>
}

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for media types that maps to FFmpeg's AVMediaType
    /// </summary>
    public enum class MediaType : int {
        Unknown = AVMEDIA_TYPE_UNKNOWN,
        Video = AVMEDIA_TYPE_VIDEO,
        Audio = AVMEDIA_TYPE_AUDIO,
    };
}
