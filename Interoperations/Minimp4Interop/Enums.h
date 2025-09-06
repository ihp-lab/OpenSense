#pragma once

using namespace System;

namespace Minimp4Interop {
    /// <summary>
    /// MP4 object type identifiers for codec specification
    /// </summary>
    public enum class ObjectType : int {
        /// <summary>
        /// MPEG-4 AAC Audio
        /// </summary>
        AAC = 0x40,

        /// <summary>
        /// H.264/AVC Video
        /// </summary>
        AVC = 0x21,

        /// <summary>
        /// H.265/HEVC Video
        /// </summary>
        HEVC = 0x23,

        /// <summary>
        /// AV1 Video
        /// </summary>
        AV1 = 0xD1,

        /// <summary>
        /// MP3 Audio
        /// </summary>
        MP3 = 0x6B,

        /// <summary>
        /// Opus Audio
        /// </summary>
        Opus = 0xAD,
    };

    /// <summary>
    /// Sample type for MP4 muxing
    /// </summary>
    public enum class SampleType : int {
        /// <summary>
        /// Non-sync sample (P-frame, B-frame, or audio)
        /// </summary>
        NonSync = 0,

        /// <summary>
        /// Sync sample (I-frame or keyframe)
        /// </summary>
        Sync = 1,
    };

    /// <summary>
    /// Track type specification
    /// </summary>
    public enum class TrackType : int {
        /// <summary>
        /// Video track
        /// </summary>
        Video = 0,

        /// <summary>
        /// Audio track
        /// </summary>
        Audio = 1,
    };

    /// <summary>
    /// MP4 muxing mode
    /// </summary>
    public enum class MuxMode : int {
        /// <summary>
        /// Default mode - uses one big mdat chunk, requires seeking back to patch size
        /// </summary>
        Default = 0,

        /// <summary>
        /// Sequential mode - no backwards seeking required
        /// </summary>
        Sequential = 1,

        /// <summary>
        /// Fragmented MP4 mode - stores track info first, spreads indexes across stream
        /// </summary>
        Fragmented = 2,
    };
}