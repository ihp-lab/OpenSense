using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HMInterop;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    // TODO: When upgrading .NET version, check for newer SIMD/Intrinsics APIs that may offer better performance.

    /// <summary>
    /// Integer RGB/YCbCr color space conversion using BT.601 coefficients.
    /// All operations use integer arithmetic only.
    /// </summary>
    internal static class ColorSpaceConverter {

        #region APIs

        #region Forward (Image → Picture)

        /// <summary>
        /// Convert an 8-bit grayscale image to Y Pel plane, filling chroma with neutral if needed.
        /// </summary>
        public static void ConvertGray8(Image image, PictureYuv picture, ChromaFormat chromaFmt) {
            picture.ReadYPlaneFromGray8(image);
            if (chromaFmt != ChromaFormat.Chroma400) {
                picture.FillNeutralChroma(8);
            }
        }

        /// <summary>
        /// Convert a 16-bit grayscale image to Y Pel plane, filling chroma with neutral if needed.
        /// </summary>
        public static void ConvertGray16(Image image, PictureYuv picture, ChromaFormat chromaFmt) {
            picture.ReadYPlaneFromGray16(image);
            if (chromaFmt != ChromaFormat.Chroma400) {
                picture.FillNeutralChroma(16);
            }
        }

        /// <summary>
        /// Convert an 8-bit RGB image to YCbCr Pel planes at the specified bit depth,
        /// with optional chroma subsampling via ChromaConverter.
        /// </summary>
        public static unsafe void ConvertColor(
            Image image, PictureYuv picture, ChromaFormat chromaFmt,
            int rOffset, int gOffset, int bOffset, int bytesPerPixel, int bitDepth
        ) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPels = new Span<int>(yPtr.ToPointer(), yStride * yH);

            if (chromaFmt == ChromaFormat.Chroma400) {
                for (var y = 0; y < height; y++) {
                    var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                    var yRow = yPels.Slice(y * yStride, width);
                    RgbRowToY(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, bitDepth);
                }
            } else {
                var (cbPtr, cbW, cbH, cbStride) = picture.GetPlaneAccess(ComponentId.Cb);
                var (crPtr, crW, crH, crStride) = picture.GetPlaneAccess(ComponentId.Cr);

                if (chromaFmt == ChromaFormat.Chroma444) {
                    var cbPels = new Span<int>(cbPtr.ToPointer(), cbStride * cbH);
                    var crPels = new Span<int>(crPtr.ToPointer(), crStride * crH);

                    for (var y = 0; y < height; y++) {
                        var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                        var yRow = yPels.Slice(y * yStride, width);
                        var cbRow = cbPels.Slice(y * cbStride, width);
                        var crRow = crPels.Slice(y * crStride, width);
                        RgbRowToYCbCr(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, cbRow, crRow, bitDepth);
                    }
                } else {
                    // 420/422: compute at 444 into packed int arrays, subsample to chroma Pel buffers
                    var pixelCount = width * height;
                    using var cbMem = MemoryPool<int>.Shared.Rent(pixelCount);
                    using var crMem = MemoryPool<int>.Shared.Rent(pixelCount);
                    var fullCb = cbMem.Memory.Span.Slice(0, pixelCount);
                    var fullCr = crMem.Memory.Span.Slice(0, pixelCount);

                    for (var y = 0; y < height; y++) {
                        var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                        var yRow = yPels.Slice(y * yStride, width);
                        var cbRow = fullCb.Slice(y * width, width);
                        var crRow = fullCr.Slice(y * width, width);
                        RgbRowToYCbCr(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, cbRow, crRow, bitDepth);
                    }

                    var cbPels = new Span<int>(cbPtr.ToPointer(), cbStride * cbH);
                    var crPels = new Span<int>(crPtr.ToPointer(), crStride * crH);
                    ChromaConverter.SubsampleToPels(fullCb, width, height, cbPels, cbW, cbH, cbStride, chromaFmt);
                    ChromaConverter.SubsampleToPels(fullCr, width, height, crPels, crW, crH, crStride, chromaFmt);
                }
            }
        }

        #endregion

        #region Inverse (YCbCr → RGB)

        /// <summary>
        /// Convert YCbCr 4:4:4 to BGR_24bpp (byte order: B, G, R).
        /// Input planes must be at the same resolution (width * height).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Alpha is set to maximum (2^bitDepth - 1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            var shift = bitDepth - 8;
            var offset16 = 16 << shift;
            var offset128 = 128 << shift;
            var maxVal = (1 << bitDepth) - 1;
            var alpha = (ushort)maxVal;

            for (var i = 0; i < pixelCount; i++) {
                var yc = y[i] - offset16;
                var cbc = cb[i] - offset128;
                var crc = cr[i] - offset128;

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

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RgbRowToY(
            ReadOnlySpan<byte> srcRow, int width, int bytesPerPixel,
            int rOffset, int gOffset, int bOffset,
            Span<int> yRow,
            int bitDepth
        ) {
            var shift = bitDepth - 8;
            for (var x = 0; x < width; x++) {
                var px = x * bytesPerPixel;
                var r = (int)srcRow[px + rOffset] << shift;
                var g = (int)srcRow[px + gOffset] << shift;
                var b = (int)srcRow[px + bOffset] << shift;
                yRow[x] = Math.Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219 << shift) + (16 << shift);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RgbRowToYCbCr(
            ReadOnlySpan<byte> srcRow, int width, int bytesPerPixel,
            int rOffset, int gOffset, int bOffset,
            Span<int> yRow, Span<int> cbRow, Span<int> crRow,
            int bitDepth
        ) {
            var shift = bitDepth - 8;
            for (var x = 0; x < width; x++) {
                var px = x * bytesPerPixel;
                var r = (int)srcRow[px + rOffset] << shift;
                var g = (int)srcRow[px + gOffset] << shift;
                var b = (int)srcRow[px + bOffset] << shift;
                yRow[x] = Math.Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219 << shift) + (16 << shift);
                cbRow[x] = Math.Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift);
                crRow[x] = Math.Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift);
            }
        }

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

            var shift = bitDepth - 8;
            var offset16 = 16 << shift;
            var offset128 = 128 << shift;

            for (var i = 0; i < pixelCount; i++) {
                var yc = y[i] - offset16;
                var cbc = cb[i] - offset128;
                var crc = cr[i] - offset128;

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
