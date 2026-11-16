#pragma once

extern "C" {
#include <libavutil/frame.h>
}

using namespace System;

namespace FFMpegInterop {
    /**
     * Flags enumeration for frame properties and states.
     * Based on FFmpeg AV_FRAME_FLAG_* definitions in libavutil/frame.h.
     */
    [Flags]
    public enum class FrameFlags : int {
        /**
         * The frame data may be corrupted, e.g. due to decoding errors.
         */
        Corrupt = AV_FRAME_FLAG_CORRUPT,

        /**
         * A flag to mark frames that are keyframes.
         */
        Key = AV_FRAME_FLAG_KEY,

        /**
         * A flag to mark the frames which need to be decoded, but shouldn't be output.
         */
        Discard = AV_FRAME_FLAG_DISCARD,

        /**
         * A flag to mark frames whose content is interlaced.
         */
        Interlaced = AV_FRAME_FLAG_INTERLACED,

        /**
         * A flag to mark frames where the top field is displayed first if the content
         * is interlaced.
         */
        TopFieldFirst = AV_FRAME_FLAG_TOP_FIELD_FIRST,

        /**
         * A decoder can use this flag to mark frames which were originally encoded losslessly.
         *
         * For coding bitstream formats which support both lossless and lossy
         * encoding, it is sometimes possible for a decoder to determine which method
         * was used when the bitstream was encoded.
         */
        Lossless = AV_FRAME_FLAG_LOSSLESS
    };
}
