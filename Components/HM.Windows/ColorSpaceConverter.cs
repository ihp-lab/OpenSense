using System;
using System.Runtime.InteropServices;

namespace OpenSense.Components.HM {
    // TODO: When upgrading .NET version, check for newer SIMD/Intrinsics APIs that may offer better performance.

    /// <summary>
    /// Integer YCbCr to RGB/BGR color space conversion using BT.601 coefficients.
    /// All operations use integer arithmetic only.
    /// </summary>
    internal static class ColorSpaceConverter {

        #region APIs

        /// <summary>
        /// Convert YCbCr 4:4:4 to BGR_24bpp (byte order: B, G, R).
        /// Input planes must be at the same resolution (width * height).
        /// </summary>
        public static void YCbCrToBgr(
            ReadOnlySpan<ushort> y, ReadOnlySpan<ushort> cb, ReadOnlySpan<ushort> cr,
            Span<byte> bgrOut,
            int width, int height, int bitDepth
        ) {
            YCbCrToChannelOrder(y, cb, cr, bgrOut, width, height, bitDepth, bOffset: 0, gOffset: 1, rOffset: 2);
        }

        /// <summary>
        /// Convert YCbCr 4:4:4 to RGB_24bpp (byte order: R, G, B).
        /// Input planes must be at the same resolution (width * height).
        /// </summary>
        public static void YCbCrToRgb(
            ReadOnlySpan<ushort> y, ReadOnlySpan<ushort> cb, ReadOnlySpan<ushort> cr,
            Span<byte> rgbOut,
            int width, int height, int bitDepth
        ) {
            YCbCrToChannelOrder(y, cb, cr, rgbOut, width, height, bitDepth, bOffset: 2, gOffset: 1, rOffset: 0);
        }

        /// <summary>
        /// Convert YCbCr 4:4:4 to RGBA_64bpp (16-bit per component, byte order: R, G, B, A).
        /// Input planes must be at the same resolution (width * height).
        /// Alpha is set to maximum (2^bitDepth - 1, then scaled to 16-bit range).
        /// </summary>
        public static void YCbCrToRgba64(
            ReadOnlySpan<ushort> y, ReadOnlySpan<ushort> cb, ReadOnlySpan<ushort> cr,
            Span<byte> rgba64Out,
            int width, int height, int bitDepth
        ) {
            var pixelCount = width * height;
            var requiredBytes = pixelCount * 8; // 4 components * 2 bytes each
            if (rgba64Out.Length < requiredBytes) {
                throw new ArgumentException($"Output buffer too small: {rgba64Out.Length} < {requiredBytes}.");
            }

            var outU16 = MemoryMarshal.Cast<byte, ushort>(rgba64Out);

            // BT.601 inverse coefficients scaled for integer math.
            // From Y'CbCr to R'G'B':
            //   R = 1.164*(Y-16) + 1.596*(Cr-128)
            //   G = 1.164*(Y-16) - 0.813*(Cr-128) - 0.392*(Cb-128)
            //   B = 1.164*(Y-16) + 2.017*(Cb-128)
            // Scaled by 256 for integer:
            //   R = (298*(Y-16) + 409*(Cr-128) + 128) >> 8
            //   G = (298*(Y-16) - 208*(Cr-128) - 100*(Cb-128) + 128) >> 8
            //   B = (298*(Y-16) + 516*(Cb-128) + 128) >> 8
            // Scale neutral/offset values by (bitDepth - 8) shift.

            var shift = bitDepth - 8;
            var offset16 = 16 << shift;
            var offset128 = 128 << shift;
            var maxVal = (1 << bitDepth) - 1;
            var alpha = (ushort)maxVal;

            // The BT.601 coefficients are for 8-bit range.
            // For higher bit depths, the formula is the same but we keep values in the higher range.
            // Result is clamped to [0, maxVal].

            for (var i = 0; i < pixelCount; i++) {
                var yc = y[i] - offset16;
                var cbc = cb[i] - offset128;
                var crc = cr[i] - offset128;

                // Scale coefficients: multiply by 298 then >> 8 for the Y term at source bit depth.
                // For bit depth > 8, the coefficients still work because all values are proportionally scaled.
                var r = (298 * yc + 409 * crc + 128) >> 8;
                var g = (298 * yc - 208 * crc - 100 * cbc + 128) >> 8;
                var b = (298 * yc + 516 * cbc + 128) >> 8;

                outU16[i * 4 + 0] = (ushort)Math.Clamp(r, 0, maxVal);
                outU16[i * 4 + 1] = (ushort)Math.Clamp(g, 0, maxVal);
                outU16[i * 4 + 2] = (ushort)Math.Clamp(b, 0, maxVal);
                outU16[i * 4 + 3] = alpha;
            }
        }

        #endregion

        private static void YCbCrToChannelOrder(
            ReadOnlySpan<ushort> y, ReadOnlySpan<ushort> cb, ReadOnlySpan<ushort> cr,
            Span<byte> output,
            int width, int height, int bitDepth,
            int rOffset, int gOffset, int bOffset
        ) {
            var pixelCount = width * height;
            var requiredBytes = pixelCount * 3;
            if (output.Length < requiredBytes) {
                throw new ArgumentException($"Output buffer too small: {output.Length} < {requiredBytes}.");
            }

            // BT.601 inverse, 8-bit range.
            // For bitDepth > 8, we need to scale the offset values.
            var shift = bitDepth - 8;
            var offset16 = 16 << shift;
            var offset128 = 128 << shift;

            for (var i = 0; i < pixelCount; i++) {
                var yc = y[i] - offset16;
                var cbc = cb[i] - offset128;
                var crc = cr[i] - offset128;

                // For 8-bit output from higher bit depth, the formula produces 8-bit range values
                // because the coefficients include an implicit normalization.
                // The 298 coefficient compensates for the limited range [16,235].
                // Result is naturally in ~[0, 255] range for valid input.
                var r = (298 * yc + 409 * crc + 128) >> (8 + shift);
                var g = (298 * yc - 208 * crc - 100 * cbc + 128) >> (8 + shift);
                var b = (298 * yc + 516 * cbc + 128) >> (8 + shift);

                var px = i * 3;
                output[px + rOffset] = (byte)Math.Clamp(r, 0, 255);
                output[px + gOffset] = (byte)Math.Clamp(g, 0, 255);
                output[px + bOffset] = (byte)Math.Clamp(b, 0, 255);
            }
        }

    }
}
