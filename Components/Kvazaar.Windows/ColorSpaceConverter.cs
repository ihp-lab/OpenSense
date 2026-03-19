using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using KvazaarInterop;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    // TODO: When upgrading .NET version, check for newer SIMD/Intrinsics APIs that may offer better performance.

    /// <summary>
    /// Integer RGB/YCbCr color space conversion using BT.601 coefficients.
    /// All operations use integer arithmetic only.
    /// The +128 >> 8 rounding is for coefficient precision and is independent of bit depth;
    /// clamp ranges and offsets scale with bit depth.
    /// </summary>
    internal static class ColorSpaceConverter {

        #region APIs

        #region Forward (Image → Picture)

        /// <summary>
        /// Convert an 8-bit grayscale image to Y ushort plane, filling chroma with neutral if needed.
        /// </summary>
        public static void ConvertGray8(Image image, Picture picture, ChromaFormat chromaFmt) {
            picture.ReadYPlaneFromGray8(image);
            if (chromaFmt != ChromaFormat.Csp400) {
                picture.FillNeutralChroma(8);
            }
        }

        /// <summary>
        /// Convert a 16-bit grayscale image to Y ushort plane, filling chroma with neutral if needed.
        /// </summary>
        public static void ConvertGray16(Image image, Picture picture, ChromaFormat chromaFmt) {
            picture.ReadYPlaneFromGray16(image);
            if (chromaFmt != ChromaFormat.Csp400) {
                picture.FillNeutralChroma(16);
            }
        }

        /// <summary>
        /// Convert an 8-bit RGB image to YCbCr ushort planes at the specified bit depth,
        /// with optional chroma subsampling via ChromaConverter.
        /// </summary>
        public static unsafe void ConvertColor(
            Image image, Picture picture, ChromaFormat chromaFmt,
            int rOffset, int gOffset, int bOffset, int bytesPerPixel, int bitDepth
        ) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);

            var (yPtr, yW, yH, yStride) = picture.GetPlaneAccess(ComponentId.Y);
            var yPixels = new Span<ushort>(yPtr.ToPointer(), yStride * yH);

            if (chromaFmt == ChromaFormat.Csp400) {
                for (var y = 0; y < height; y++) {
                    var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                    var yRow = yPixels.Slice(y * yStride, width);
                    RgbRowToY(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, bitDepth);
                }
            } else if (chromaFmt == ChromaFormat.Csp444) {
                var (cbPtr, cbW, cbH, cbStride) = picture.GetPlaneAccess(ComponentId.U);
                var (crPtr, crW, crH, crStride) = picture.GetPlaneAccess(ComponentId.V);
                var cbPixels = new Span<ushort>(cbPtr.ToPointer(), cbStride * cbH);
                var crPixels = new Span<ushort>(crPtr.ToPointer(), crStride * crH);

                for (var y = 0; y < height; y++) {
                    var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                    var yRow = yPixels.Slice(y * yStride, width);
                    var cbRow = cbPixels.Slice(y * cbStride, width);
                    var crRow = crPixels.Slice(y * crStride, width);
                    RgbRowToYCbCr(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, cbRow, crRow, bitDepth);
                }
            } else {
                // 420/422: compute at 444 into packed int arrays, subsample to chroma planes
                var pixelCount = width * height;
                using var cbMem = MemoryPool<int>.Shared.Rent(pixelCount);
                using var crMem = MemoryPool<int>.Shared.Rent(pixelCount);
                var cbSpan = cbMem.Memory.Span.Slice(0, pixelCount);
                var crSpan = crMem.Memory.Span.Slice(0, pixelCount);

                for (var y = 0; y < height; y++) {
                    var srcRow = src.Slice(y * imgStride, width * bytesPerPixel);
                    var yRow = yPixels.Slice(y * yStride, width);
                    var cbRow = cbSpan.Slice(y * width, width);
                    var crRow = crSpan.Slice(y * width, width);
                    RgbRowToYCbCr(srcRow, width, bytesPerPixel, rOffset, gOffset, bOffset, yRow, cbRow, crRow, bitDepth);
                }

                ChromaConverter.SubsampleToPlane(cbSpan, width, height, picture, ComponentId.U, chromaFmt);
                ChromaConverter.SubsampleToPlane(crSpan, width, height, picture, ComponentId.V, chromaFmt);
            }
        }

        #endregion

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RgbRowToY(
            ReadOnlySpan<byte> srcRow, int width, int bytesPerPixel,
            int rOffset, int gOffset, int bOffset,
            Span<ushort> yRow,
            int bitDepth
        ) {
            var shift = bitDepth - 8;
            for (var x = 0; x < width; x++) {
                var px = x * bytesPerPixel;
                var r = (int)srcRow[px + rOffset] << shift;
                var g = (int)srcRow[px + gOffset] << shift;
                var b = (int)srcRow[px + bOffset] << shift;
                yRow[x] = (ushort)(Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219 << shift) + (16 << shift));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RgbRowToYCbCr(
            ReadOnlySpan<byte> srcRow, int width, int bytesPerPixel,
            int rOffset, int gOffset, int bOffset,
            Span<ushort> yRow, Span<int> cbRow, Span<int> crRow,
            int bitDepth
        ) {
            var shift = bitDepth - 8;
            for (var x = 0; x < width; x++) {
                var px = x * bytesPerPixel;
                var r = (int)srcRow[px + rOffset] << shift;
                var g = (int)srcRow[px + gOffset] << shift;
                var b = (int)srcRow[px + bOffset] << shift;
                yRow[x] = (ushort)(Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219 << shift) + (16 << shift));
                cbRow[x] = Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift);
                crRow[x] = Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RgbRowToYCbCr(
            ReadOnlySpan<byte> srcRow, int width, int bytesPerPixel,
            int rOffset, int gOffset, int bOffset,
            Span<ushort> yRow, Span<ushort> cbRow, Span<ushort> crRow,
            int bitDepth
        ) {
            var shift = bitDepth - 8;
            for (var x = 0; x < width; x++) {
                var px = x * bytesPerPixel;
                var r = (int)srcRow[px + rOffset] << shift;
                var g = (int)srcRow[px + gOffset] << shift;
                var b = (int)srcRow[px + bOffset] << shift;
                yRow[x] = (ushort)(Clamp((66 * r + 129 * g + 25 * b + 128) >> 8, 0, 219 << shift) + (16 << shift));
                cbRow[x] = (ushort)(Clamp((-38 * r - 74 * g + 112 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift));
                crRow[x] = (ushort)(Clamp((112 * r - 94 * g - 18 * b + 128) >> 8, -(112 << shift), 112 << shift) + (128 << shift));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Clamp(int value, int min, int max) {
            if (value < min) {
                return min;
            }
            if (value > max) {
                return max;
            }
            return value;
        }
    }
}
