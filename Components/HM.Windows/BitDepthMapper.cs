using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenSense.Components.HM {
    // TODO: When upgrading .NET version, check for newer SIMD/Intrinsics APIs that may offer better performance.

    /// <summary>
    /// Bidirectional integer bit depth mapping.
    /// Maps a window of the source value range to the target range using shift (no floating point).
    /// </summary>
    /// <remarks>
    /// Unified model:
    ///   scaleShift &gt; 0: result = clamp((source - windowStart) >> scaleShift, 0, maxTarget)   [downscale]
    ///   scaleShift &lt; 0: result = clamp((source - windowStart) &lt;&lt; -scaleShift, 0, maxTarget) [upscale]
    ///   scaleShift = 0: result = clamp(source - windowStart, 0, maxTarget)                     [1:1 + clamp]
    ///
    /// Window size = 2^(targetBits + scaleShift).
    /// </remarks>
    internal static class BitDepthMapper {

        #region APIs

        /// <summary>
        /// In-place mapping on Pel (int) buffer with stride.
        /// </summary>
        public static void MapPlane(Span<int> pels, int width, int height, int stride, int targetBits, int scaleShift, int windowStart) {
            var maxVal = (1 << targetBits) - 1;

            if (scaleShift >= 0) {
                MapPlaneDown(pels, width, height, stride, scaleShift, windowStart, maxVal);
            } else {
                MapPlaneUp(pels, width, height, stride, -scaleShift, windowStart, maxVal);
            }
        }

        /// <summary>
        /// Fused mapping from Pel (int) buffer to byte destination.
        /// </summary>
        public static void MapPlaneToBytes(ReadOnlySpan<int> pels, int width, int height, int stride, Span<byte> dest, int targetBits, int scaleShift, int windowStart) {
            var maxVal = (1 << targetBits) - 1;
            var maxByte = (byte)Math.Min(maxVal, 255);

            var destOffset = 0;
            if (scaleShift >= 0) {
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = MapValueDownToByte(row[x], windowStart, scaleShift, maxByte);
                    }
                }
            } else {
                var leftShift = -scaleShift;
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = MapValueUpToByte(row[x], windowStart, leftShift, maxByte);
                    }
                }
            }
        }

        /// <summary>
        /// Fused mapping from Pel (int) buffer to ushort destination.
        /// </summary>
        public static void MapPlaneToUshorts(ReadOnlySpan<int> pels, int width, int height, int stride, Span<ushort> dest, int targetBits, int scaleShift, int windowStart) {
            var maxVal = (1 << targetBits) - 1;

            var destOffset = 0;
            if (scaleShift >= 0) {
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = (ushort)MapValueDown(row[x], windowStart, scaleShift, maxVal);
                    }
                }
            } else {
                var leftShift = -scaleShift;
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = (ushort)MapValueUp(row[x], windowStart, leftShift, maxVal);
                    }
                }
            }
        }

        #endregion

        #region Downscale (scaleShift >= 0, right-shift)

        private static void MapPlaneDown(Span<int> pels, int width, int height, int stride, int shift, int windowStart, int maxVal) {
            var vecSize = Vector<int>.Count;
            for (var y = 0; y < height; y++) {
                var row = pels.Slice(y * stride, width);
                if (row.Length < vecSize) {
                    for (var x = 0; x < row.Length; x++) {
                        row[x] = MapValueDown(row[x], windowStart, shift, maxVal);
                    }
                } else {
                    var vecWinStart = new Vector<int>(windowStart);
                    var vecMax = new Vector<int>(maxVal);
                    var vecZero = Vector<int>.Zero;

                    var x = 0;
                    var alignedLen = row.Length - (row.Length % vecSize);
                    for (; x < alignedLen; x += vecSize) {
                        var slice = row.Slice(x, vecSize);
                        var vec = new Vector<int>(slice);
                        var sub = Vector.Subtract(vec, vecWinStart);
                        var underflow = Vector.LessThan(vec, vecWinStart);
                        sub = Vector.ConditionalSelect(underflow, vecZero, sub);
                        if (shift > 0) {
                            sub = Vector.ShiftRightLogical(sub, shift);
                        }
                        sub = Vector.Min(sub, vecMax);
                        sub.CopyTo(slice);
                    }
                    for (; x < row.Length; x++) {
                        row[x] = MapValueDown(row[x], windowStart, shift, maxVal);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapValueDown(int value, int windowStart, int shift, int maxVal) {
            if (value < windowStart) {
                return 0;
            }
            var mapped = (value - windowStart) >> shift;
            return mapped > maxVal ? maxVal : mapped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte MapValueDownToByte(int value, int windowStart, int shift, byte maxVal) {
            if (value < windowStart) {
                return 0;
            }
            var mapped = (value - windowStart) >> shift;
            return mapped > maxVal ? maxVal : (byte)mapped;
        }

        #endregion

        #region Upscale (scaleShift < 0, left-shift)

        private static void MapPlaneUp(Span<int> pels, int width, int height, int stride, int leftShift, int windowStart, int maxVal) {
            var vecSize = Vector<int>.Count;
            for (var y = 0; y < height; y++) {
                var row = pels.Slice(y * stride, width);
                if (row.Length < vecSize) {
                    for (var x = 0; x < row.Length; x++) {
                        row[x] = MapValueUp(row[x], windowStart, leftShift, maxVal);
                    }
                } else {
                    var vecWinStart = new Vector<int>(windowStart);
                    var vecMax = new Vector<int>(maxVal);
                    var vecZero = Vector<int>.Zero;

                    var x = 0;
                    var alignedLen = row.Length - (row.Length % vecSize);
                    for (; x < alignedLen; x += vecSize) {
                        var slice = row.Slice(x, vecSize);
                        var vec = new Vector<int>(slice);
                        var sub = Vector.Subtract(vec, vecWinStart);
                        var underflow = Vector.LessThan(vec, vecWinStart);
                        sub = Vector.ConditionalSelect(underflow, vecZero, sub);
                        sub = Vector.ShiftLeft(sub, leftShift);
                        sub = Vector.Min(sub, vecMax);
                        sub.CopyTo(slice);
                    }
                    for (; x < row.Length; x++) {
                        row[x] = MapValueUp(row[x], windowStart, leftShift, maxVal);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapValueUp(int value, int windowStart, int leftShift, int maxVal) {
            if (value < windowStart) {
                return 0;
            }
            var mapped = (value - windowStart) << leftShift;
            return mapped > maxVal ? maxVal : mapped;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte MapValueUpToByte(int value, int windowStart, int leftShift, byte maxVal) {
            if (value < windowStart) {
                return 0;
            }
            var mapped = (value - windowStart) << leftShift;
            return mapped > maxVal ? maxVal : (byte)mapped;
        }

        #endregion
    }
}
