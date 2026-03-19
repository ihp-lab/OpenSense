using System;
using KvazaarInterop;

namespace OpenSense.Components.Kvazaar {
    /// <summary>
    /// Chroma subsampling utilities for Kvazaar Picture (kvz_pixel = ushort).
    /// Operates on ushort pixel buffers, unlike HM's ChromaConverter which operates on int (Pel) buffers.
    /// </summary>
    internal static class ChromaConverter {

        #region APIs

        /// <summary>
        /// Downsample packed (stride=width) int source to strided ushort destination using block averaging.
        /// Source is int (from RGB→YCbCr computation), destination is ushort (kvz_pixel) with stride.
        /// Output values are in 8-bit BT.601 range. Use BitDepthMapping for bit depth conversion.
        /// </summary>
        public static unsafe void SubsampleToPlane(ReadOnlySpan<int> src, int srcW, int srcH, Picture picture, ComponentId componentId, ChromaFormat chromaFmt) {
            var (dstPtr, dstW, dstH, dstStride) = picture.GetPlaneAccess(componentId);
            var dst = new Span<ushort>(dstPtr.ToPointer(), dstStride * dstH);

            if (chromaFmt == ChromaFormat.Csp420) {
                for (var cy = 0; cy < dstH; cy++) {
                    var sy = cy * 2;
                    var sy1 = Math.Min(sy + 1, srcH - 1);
                    var dstRow = dst.Slice(cy * dstStride, dstW);
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = src[sy * srcW + sx] + src[sy * srcW + sx1] + src[sy1 * srcW + sx] + src[sy1 * srcW + sx1];
                        dstRow[cx] = (ushort)((sum + 2) / 4);
                    }
                }
            } else if (chromaFmt == ChromaFormat.Csp422) {
                for (var cy = 0; cy < dstH; cy++) {
                    var dstRow = dst.Slice(cy * dstStride, dstW);
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = src[cy * srcW + sx] + src[cy * srcW + sx1];
                        dstRow[cx] = (ushort)((sum + 1) / 2);
                    }
                }
            }
        }

        #endregion
    }
}
