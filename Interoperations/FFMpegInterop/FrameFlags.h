#pragma once

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
        Corrupt = 1 << 0,

        /**
         * A flag to mark frames that are keyframes.
         */
        Key = 1 << 1,

        /**
         * A flag to mark the frames which need to be decoded, but shouldn't be output.
         */
        Discard = 1 << 2,

        /**
         * A flag to mark frames whose content is interlaced.
         */
        Interlaced = 1 << 3,

        /**
         * A flag to mark frames where the top field is displayed first if the content
         * is interlaced.
         */
        TopFieldFirst = 1 << 4,

        /**
         * A decoder can use this flag to mark frames which were originally encoded losslessly.
         *
         * For coding bitstream formats which support both lossless and lossy
         * encoding, it is sometimes possible for a decoder to determine which method
         * was used when the bitstream was encoded.
         */
        Lossless = 1 << 5
    };
}
