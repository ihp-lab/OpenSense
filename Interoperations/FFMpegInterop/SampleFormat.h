#pragma once

extern "C" {
#include <libavutil/samplefmt.h>
}

namespace FFMpegInterop {
    /// <summary>
    /// Managed enum for interleaved sample formats that maps to FFmpeg's AVSampleFormat
    /// </summary>
    public enum class SampleFormat : int {
        None = AV_SAMPLE_FMT_NONE,
        U8 = AV_SAMPLE_FMT_U8,
        S16 = AV_SAMPLE_FMT_S16,
        S32 = AV_SAMPLE_FMT_S32,
        FLT = AV_SAMPLE_FMT_FLT,
        DBL = AV_SAMPLE_FMT_DBL,
    };
}
