#pragma once

#include <cstdint>

// HEVC NAL unit type constants (ITU-T H.265 Table 7-1)
static constexpr int32_t HEVC_NAL_TYPE_VPS = 32;
static constexpr int32_t HEVC_NAL_TYPE_SPS = 33;
static constexpr int32_t HEVC_NAL_TYPE_PPS = 34;

// Extract HEVC NAL unit type from the first byte of NAL header.
static inline int32_t HevcNalType(const unsigned char* nalData) {
    return (nalData[0] >> 1) & 0x3F;
}

// Whether a NAL unit requires a 4-byte start code (00 00 00 01) per Annex B rules.
static inline bool HevcNeedsLongStartCode(const unsigned char* nalData, bool isFirst) {
    if (isFirst) {
        return true;
    }
    auto nalType = HevcNalType(nalData);
    return nalType >= HEVC_NAL_TYPE_VPS && nalType <= HEVC_NAL_TYPE_PPS;
}

// Native NAL unit info for passing between native encode and AccessUnit factory.
// Used in unmanaged code only.
struct NativeNalInfo {
    const unsigned char* data;
    int32_t size;
    bool longStartCode; // true = 4-byte (00 00 00 01), false = 3-byte (00 00 01)
};
