using System;
using HMInterop;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Lightweight HEVC SPS parser for extracting basic video parameters.
    /// Only parses fields up to pic_width/height_in_luma_samples.
    /// </summary>
    internal static class SequenceParameterSetParser {

        private const int NalTypeSps = 33;
        private const int NalTypeShift = 1;
        private const int NalTypeMask = 0x3F;

        #region APIs
        /// <summary>
        /// Find the first SPS NAL in an AccessUnit and extract dimensions.
        /// </summary>
        public static bool TryGetDimensionsFromAccessUnit(AccessUnit au, out int width, out int height) {
            width = 0;
            height = 0;

            for (var i = 0; i < au.Count; i++) {
                var nalData = au[i];
                if (nalData.Length < 2) {
                    continue;
                }
                var nalType = (nalData.Span[0] >> NalTypeShift) & NalTypeMask;
                if (nalType == NalTypeSps && TryGetDimensions(nalData.Span, out width, out height)) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Try to extract video dimensions from a raw SPS NAL unit (without start code).
        /// Returns false if the data is not a valid HEVC SPS or parsing fails.
        /// </summary>
        private static bool TryGetDimensions(ReadOnlySpan<byte> spsNalData, out int width, out int height) {
            width = 0;
            height = 0;

            if (spsNalData.Length < 4) {
                return false;
            }

            // Verify NAL type is SPS (33)
            var nalType = (spsNalData[0] >> NalTypeShift) & NalTypeMask;
            if (nalType != NalTypeSps) {
                return false;
            }

            // Skip NAL header (2 bytes), remove emulation prevention bytes (00 00 03 → 00 00)
            var rbsp = RemoveEmulationPreventionBytes(spsNalData.Slice(2));
            var reader = new BitReader(rbsp);

            try {
                // sps_video_parameter_set_id: u(4)
                reader.SkipBits(4);

                // sps_max_sub_layers_minus1: u(3)
                var maxSubLayersMinus1 = reader.ReadBits(3);

                // sps_temporal_id_nesting_flag: u(1)
                reader.SkipBits(1);

                // profile_tier_level(1, maxSubLayersMinus1)
                SkipProfileTierLevel(ref reader, maxSubLayersMinus1);

                // sps_seq_parameter_set_id: ue(v)
                reader.ReadExpGolomb();

                // chroma_format_idc: ue(v)
                var chromaFormatIdc = reader.ReadExpGolomb();
                if (chromaFormatIdc > 3) {
                    return false;
                }

                // if chroma_format_idc == 3: separate_colour_plane_flag: u(1)
                if (chromaFormatIdc == 3) {
                    reader.ReadBits(1);
                }

                // pic_width_in_luma_samples: ue(v)
                width = (int)reader.ReadExpGolomb();

                // pic_height_in_luma_samples: ue(v)
                height = (int)reader.ReadExpGolomb();

                return width > 0 && height > 0;
            } catch {
                return false;
            }
        }

        private static void SkipProfileTierLevel(ref BitReader reader, uint maxSubLayersMinus1) {
            // general_profile_space(2) + general_tier_flag(1) + general_profile_idc(5)
            // + general_profile_compatibility_flag[32]
            // + 4 common constraint flags + conditional constraint flags(43) + inbld_flag(1) = 48
            // + general_level_idc(8)
            // = 96 bits total (same for all profiles; see HM TDecCAVLC.cpp:1634-1707)
            reader.SkipBits(96);

            // sub_layer_profile_present_flag[i] + sub_layer_level_present_flag[i]
            var subLayerProfilePresent = new bool[maxSubLayersMinus1];
            var subLayerLevelPresent = new bool[maxSubLayersMinus1];
            for (var i = 0u; i < maxSubLayersMinus1; i++) {
                subLayerProfilePresent[i] = reader.ReadBits(1) != 0;
                subLayerLevelPresent[i] = reader.ReadBits(1) != 0;
            }

            // reserved_zero_2bits padding to 8 sub_layer slots
            if (maxSubLayersMinus1 > 0) {
                reader.SkipBits((int)(8 - maxSubLayersMinus1) * 2);
            }

            // sub_layer profile/level data
            for (var i = 0u; i < maxSubLayersMinus1; i++) {
                if (subLayerProfilePresent[i]) {
                    // sub_layer profile: 2+1+5+32+48 = 88 bits (same structure as general minus level_idc)
                    reader.SkipBits(88);
                }
                if (subLayerLevelPresent[i]) {
                    reader.SkipBits(8);
                }
            }
        }

        private static byte[] RemoveEmulationPreventionBytes(ReadOnlySpan<byte> data) {
            var result = new byte[data.Length];
            var j = 0;
            for (var i = 0; i < data.Length; i++) {
                if (i >= 2 && data[i] == 0x03 && data[i - 1] == 0x00 && data[i - 2] == 0x00) {
                    continue;
                }
                result[j++] = data[i];
            }
            return result[..j];
        }

        #region Classes
        private ref struct BitReader {

            private readonly ReadOnlySpan<byte> _data;

            public int BitOffset { get; private set; }

            public BitReader(ReadOnlySpan<byte> data) {
                _data = data;
                BitOffset = 0;
            }

            public void SkipBits(int count) {
                if (BitOffset + count > _data.Length * 8) {
                    throw new InvalidOperationException("Unexpected end of SPS data.");
                }
                BitOffset += count;
            }

            public uint ReadBits(int count) {
                var result = 0u;
                for (var i = 0; i < count; i++) {
                    var byteIndex = BitOffset >> 3;
                    var bitIndex = 7 - (BitOffset & 7);
                    if (byteIndex >= _data.Length) {
                        throw new InvalidOperationException("Unexpected end of SPS data.");
                    }
                    result = (result << 1) | ((uint)(_data[byteIndex] >> bitIndex) & 1);
                    BitOffset++;
                }
                return result;
            }

            public uint ReadExpGolomb() {
                var leadingZeros = 0;
                while (ReadBits(1) == 0) {
                    leadingZeros++;
                    if (leadingZeros >= sizeof(uint) * 8 - 1) {
                        throw new InvalidOperationException("Invalid Exp-Golomb code in SPS.");
                    }
                }
                if (leadingZeros == 0) {
                    return 0;
                }
                return (1u << leadingZeros) - 1 + ReadBits(leadingZeros);
            }
        } 
        #endregion
    }
}
