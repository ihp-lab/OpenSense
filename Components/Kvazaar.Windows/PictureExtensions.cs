using System;
using KvazaarInterop;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    /// <summary>
    /// Extension methods for Kvazaar Picture pixel buffer operations via GetPlaneAccess.
    /// Operates on kvz_pixel (ushort for 16-bit build) buffers.
    /// </summary>
    internal static class PictureExtensions {

        #region APIs

        /// <summary>
        /// Fill U and V planes with neutral chroma value (128 scaled to bit depth).
        /// </summary>
        public static unsafe void FillNeutralChroma(this Picture picture, int bitDepth) {
            var neutralValue = (ushort)(128 << Math.Max(bitDepth - 8, 0));
            foreach (var comp in new[] { ComponentId.U, ComponentId.V }) {
                var (ptr, w, h, stride) = picture.GetPlaneAccess(comp);
                var pixels = new Span<ushort>(ptr.ToPointer(), stride * h);
                for (var y = 0; y < h; y++) {
                    pixels.Slice(y * stride, w).Fill(neutralValue);
                }
            }
        }

        /// <summary>
        /// Copy 8-bit grayscale pixels from an ImageBase into the Y plane (byte → ushort widening).
        /// </summary>
        public static unsafe void ReadYPlaneFromGray8(this Picture dest, ImageBase image) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;
            var src = new ReadOnlySpan<byte>(image.ImageData.ToPointer(), imgStride * height);

            var (yPtr, yW, yH, yStride) = dest.GetPlaneAccess(ComponentId.Y);
            var yPixels = new Span<ushort>(yPtr.ToPointer(), yStride * yH);
            for (var y = 0; y < height; y++) {
                var srcRow = src.Slice(y * imgStride, width);
                var dstRow = yPixels.Slice(y * yStride, width);
                for (var x = 0; x < width; x++) {
                    dstRow[x] = srcRow[x];
                }
            }
        }

        /// <summary>
        /// Copy 16-bit grayscale pixels from an ImageBase into the Y plane (ushort → ushort copy).
        /// </summary>
        public static unsafe void ReadYPlaneFromGray16(this Picture dest, ImageBase image) {
            var width = image.Width;
            var height = image.Height;
            var imgStride = image.Stride;

            var (yPtr, yW, yH, yStride) = dest.GetPlaneAccess(ComponentId.Y);
            var yPixels = new Span<ushort>(yPtr.ToPointer(), yStride * yH);
            for (var y = 0; y < height; y++) {
                var srcRow = new ReadOnlySpan<ushort>((byte*)image.ImageData.ToPointer() + y * imgStride, width);
                var dstRow = yPixels.Slice(y * yStride, width);
                srcRow.CopyTo(dstRow);
            }
        }

        #endregion
    }
}
