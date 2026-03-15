using System;
using System.Buffers;
using HMInterop;

namespace OpenSense.Components.HM {
    // TODO: When upgrading .NET version, check for newer SIMD/Intrinsics APIs that may offer better performance.

    /// <summary>
    /// Converts PictureYuv between chroma formats (400/420/422/444).
    /// Operates directly on Pel (int) buffers via GetPlaneAccess.
    /// </summary>
    internal static class ChromaConverter {

        private static readonly ComponentId[] ChromaComponents = { ComponentId.Cb, ComponentId.Cr };

        #region APIs

        /// <summary>
        /// Convert a PictureYuv to the target chroma format.
        /// Returns the source unchanged if formats already match.
        /// The caller is responsible for disposing the returned PictureYuv if it differs from the source.
        /// </summary>
        public static PictureYuv Convert(PictureYuv source, ChromaFormat targetFormat, ChromaUpsampleMethod upsampleMethod, int bitDepth) {
            if (source.ChromaFormat == targetFormat) {
                return source;
            }

            var width = source.Width;
            var height = source.Height;
            var result = new PictureYuv(targetFormat, width, height);

            try {
                // Y plane: always copy as-is (direct Pel→Pel)
                source.CopyPlaneTo(ComponentId.Y, result, ComponentId.Y);

                // Chroma planes
                if (targetFormat == ChromaFormat.Chroma400) {
                    // Color → grayscale: just discard chroma (Y is luminance)
                } else if (source.ChromaFormat == ChromaFormat.Chroma400) {
                    // Grayscale → color: fill chroma with neutral
                    result.FillNeutralChroma(bitDepth);
                } else {
                    // Color → color: resample chroma planes
                    ConvertChromaPlanes(source, result, upsampleMethod);
                }
            } catch {
                result.Dispose();
                throw;
            }

            return result;
        }

        /// <summary>
        /// Downsample packed (stride=width) source to strided destination using block averaging.
        /// </summary>
        public static void SubsampleToPels(ReadOnlySpan<int> src, int srcW, int srcH, Span<int> dst, int dstW, int dstH, int dstStride, ChromaFormat chromaFmt) {
            if (chromaFmt == ChromaFormat.Chroma420) {
                for (var cy = 0; cy < dstH; cy++) {
                    var sy = cy * 2;
                    var sy1 = Math.Min(sy + 1, srcH - 1);
                    var dstRow = dst.Slice(cy * dstStride, dstW);
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = src[sy * srcW + sx] + src[sy * srcW + sx1] + src[sy1 * srcW + sx] + src[sy1 * srcW + sx1];
                        dstRow[cx] = (sum + 2) / 4;
                    }
                }
            } else if (chromaFmt == ChromaFormat.Chroma422) {
                for (var cy = 0; cy < dstH; cy++) {
                    var dstRow = dst.Slice(cy * dstStride, dstW);
                    for (var cx = 0; cx < dstW; cx++) {
                        var sx = cx * 2;
                        var sx1 = Math.Min(sx + 1, srcW - 1);
                        var sum = src[cy * srcW + sx] + src[cy * srcW + sx1];
                        dstRow[cx] = (sum + 1) / 2;
                    }
                }
            }
        }

        #endregion

        private static unsafe void ConvertChromaPlanes(PictureYuv source, PictureYuv dest, ChromaUpsampleMethod upsampleMethod) {
            foreach (var comp in ChromaComponents) {
                var (srcPtr, srcW, srcH, srcStride) = source.GetPlaneAccess(comp);
                var (dstPtr, dstW, dstH, dstStride) = dest.GetPlaneAccess(comp);
                var srcPels = new ReadOnlySpan<int>(srcPtr.ToPointer(), srcStride * srcH);
                var dstPels = new Span<int>(dstPtr.ToPointer(), dstStride * dstH);

                ResamplePlane(srcPels, srcW, srcH, srcStride, dstPels, dstW, dstH, dstStride, upsampleMethod);
            }
        }

        /// <summary>
        /// Resample a plane from source to destination dimensions, operating directly on Pel buffers with stride.
        /// </summary>
        private static void ResamplePlane(
            ReadOnlySpan<int> src, int srcW, int srcH, int srcStride,
            Span<int> dst, int dstW, int dstH, int dstStride,
            ChromaUpsampleMethod upsampleMethod
        ) {
            if (srcW == dstW && srcH == dstH) {
                // Same dimensions, direct copy row-by-row
                for (var y = 0; y < srcH; y++) {
                    src.Slice(y * srcStride, srcW).CopyTo(dst.Slice(y * dstStride, srcW));
                }
                return;
            }

            if (dstW >= srcW && dstH >= srcH) {
                Upsample(src, srcW, srcH, srcStride, dst, dstW, dstH, dstStride, upsampleMethod);
            } else if (dstW <= srcW && dstH <= srcH) {
                Downsample(src, srcW, srcH, srcStride, dst, dstW, dstH, dstStride);
            } else {
                // Mixed: upsample to intermediate, then downsample
                var fullW = Math.Max(srcW, dstW);
                var fullH = Math.Max(srcH, dstH);
                var tmpBuf = ArrayPool<int>.Shared.Rent(fullW * fullH);
                try {
                    var tmp = tmpBuf.AsSpan(0, fullW * fullH);
                    // Upsample to packed intermediate (stride = width)
                    Upsample(src, srcW, srcH, srcStride, tmp, fullW, fullH, fullW, upsampleMethod);
                    // Downsample from packed intermediate to strided destination
                    Downsample(tmp, fullW, fullH, fullW, dst, dstW, dstH, dstStride);
                } finally {
                    ArrayPool<int>.Shared.Return(tmpBuf);
                }
            }
        }

        private static void Upsample(
            ReadOnlySpan<int> src, int srcW, int srcH, int srcStride,
            Span<int> dst, int dstW, int dstH, int dstStride,
            ChromaUpsampleMethod method
        ) {
            switch (method) {
                case ChromaUpsampleMethod.NearestNeighbor:
                    UpsampleNearest(src, srcW, srcH, srcStride, dst, dstW, dstH, dstStride);
                    break;
                case ChromaUpsampleMethod.Bilinear:
                    UpsampleBilinear(src, srcW, srcH, srcStride, dst, dstW, dstH, dstStride);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported upsample method: {method}");
            }
        }

        private static void UpsampleNearest(
            ReadOnlySpan<int> src, int srcW, int srcH, int srcStride,
            Span<int> dst, int dstW, int dstH, int dstStride
        ) {
            var scaleX = dstW / srcW;
            var scaleY = dstH / srcH;
            for (var dy = 0; dy < dstH; dy++) {
                var sy = Math.Min(dy / scaleY, srcH - 1);
                var srcRow = src.Slice(sy * srcStride, srcW);
                var dstRow = dst.Slice(dy * dstStride, dstW);
                for (var dx = 0; dx < dstW; dx++) {
                    var sx = Math.Min(dx / scaleX, srcW - 1);
                    dstRow[dx] = srcRow[sx];
                }
            }
        }

        private static void UpsampleBilinear(
            ReadOnlySpan<int> src, int srcW, int srcH, int srcStride,
            Span<int> dst, int dstW, int dstH, int dstStride
        ) {
            for (var dy = 0; dy < dstH; dy++) {
                var syNumer = dy * (srcH - 1);
                var syDenom = Math.Max(dstH - 1, 1);
                var sy0 = syNumer / syDenom;
                var sy1 = Math.Min(sy0 + 1, srcH - 1);
                var fracY = syNumer - sy0 * syDenom;

                var srcRow0 = src.Slice(sy0 * srcStride, srcW);
                var srcRow1 = src.Slice(sy1 * srcStride, srcW);
                var dstRow = dst.Slice(dy * dstStride, dstW);

                for (var dx = 0; dx < dstW; dx++) {
                    var sxNumer = dx * (srcW - 1);
                    var sxDenom = Math.Max(dstW - 1, 1);
                    var sx0 = sxNumer / sxDenom;
                    var sx1 = Math.Min(sx0 + 1, srcW - 1);
                    var fracX = sxNumer - sx0 * sxDenom;

                    var v00 = srcRow0[sx0];
                    var v10 = srcRow0[sx1];
                    var v01 = srcRow1[sx0];
                    var v11 = srcRow1[sx1];

                    var invFracX = sxDenom - fracX;
                    var invFracY = syDenom - fracY;
                    var total = (long)v00 * invFracX * invFracY
                              + (long)v10 * fracX * invFracY
                              + (long)v01 * invFracX * fracY
                              + (long)v11 * fracX * fracY;
                    var denom = (long)sxDenom * syDenom;
                    dstRow[dx] = (int)((total + denom / 2) / denom);
                }
            }
        }

        private static void Downsample(
            ReadOnlySpan<int> src, int srcW, int srcH, int srcStride,
            Span<int> dst, int dstW, int dstH, int dstStride
        ) {
            var blockW = srcW / dstW;
            var blockH = srcH / dstH;
            var blockSize = blockW * blockH;
            var halfBlock = blockSize / 2;

            for (var dy = 0; dy < dstH; dy++) {
                var sy = dy * blockH;
                var dstRow = dst.Slice(dy * dstStride, dstW);
                for (var dx = 0; dx < dstW; dx++) {
                    var sx = dx * blockW;
                    var sum = 0L;
                    for (var by = 0; by < blockH; by++) {
                        var srcRow = src.Slice(Math.Min(sy + by, srcH - 1) * srcStride, srcW);
                        for (var bx = 0; bx < blockW; bx++) {
                            sum += srcRow[Math.Min(sx + bx, srcW - 1)];
                        }
                    }
                    dstRow[dx] = (int)((sum + halfBlock) / blockSize);
                }
            }
        }

    }
}
