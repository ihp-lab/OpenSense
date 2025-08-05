#pragma once

namespace FFMpegInterop {
    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/nvenc.h" />.
    /// </remarks>
    public enum class NvencHevcPreset {
        Deafult,
        Slow,
        Medium,
        Fast,
        HP,
        HQ,
        BD,
        LowLatencyDefault,
        LowLatencyHQ,
        LowLatencyHP,
        LosslessDefault,
        LosslessHP,
        P1,
        P2,
        P3,
        P4,
        P5,
        P6,
        P7,
    };

    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/nv-codec-headers/blob/master/include/ffnvcodec/nvEncodeAPI.h" />.
    /// </remarks>
    public enum class NvencHevcTuningInfo {
        Undefined,
        HighQuality,
        LowLatency, 
        UltraLowLatency,                        
        Lossless,
        UltraHighQuality,
    };

    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/nvenc.h" />.
    /// </remarks>
    public enum class NvencHevcProfile {
        Main,
        Main10,
        REXT,
        MultiviewMain,
    };

    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/nv-codec-headers/blob/master/include/ffnvcodec/nvEncodeAPI.h" />.
    /// </remarks>
    public enum class NvencHevcLevel {
        AutoSelect,
        L10 = 30,
        L20 = 60,
        L21 = 63,
        L30 = 90,
        L31 = 93,
        L40 = 120,
        L41 = 123,
        L50 = 150,
        L51 = 153,
        L52 = 156,
        L60 = 180,
        L61 = 183,
        L62 = 186,
    };

    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/nv-codec-headers/blob/master/include/ffnvcodec/nvEncodeAPI.h" />.
    /// </remarks>
    public enum class NvencHevcTier {
        Main,
        High,
    };

    /// <remarks>
    /// Values are from <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/nvenc.h" />.
    /// </remarks>
    public enum class NvencHevcRgbMode {
        Disabled,
        Yuv420,
        Yuv444,
    };
}