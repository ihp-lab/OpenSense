using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Bidirectional integer bit depth mapping.
    /// Maps a window of the source value range to the target range using shift and offset.
    /// </summary>
    /// <remarks>
    /// Model:
    ///   scaleShift &gt; 0: mapped = (source - inputStart) >> scaleShift   [downscale]
    ///   scaleShift &lt; 0: mapped = (source - inputStart) &lt;&lt; -scaleShift [upscale]
    ///   scaleShift = 0: mapped = source - inputStart                     [1:1]
    ///   result = clamp(mapped + outputStart, 0, maxTarget)
    /// </remarks>
    internal static class BitDepthMapper {

        #region APIs

        /// <summary>
        /// In-place mapping on Pel (int) buffer with stride.
        /// </summary>
        public static void MapPlane(Span<int> pels, int width, int height, int stride, int targetBits, int scaleShift, int inputStart, int outputStart) {
            var maxVal = (1 << targetBits) - 1;

            if (scaleShift >= 0) {
                MapPlaneDown(pels, width, height, stride, scaleShift, inputStart, outputStart, maxVal);
            } else {
                MapPlaneUp(pels, width, height, stride, -scaleShift, inputStart, outputStart, maxVal);
            }
        }

        /// <summary>
        /// Fused mapping from Pel (int) buffer to byte destination.
        /// </summary>
        public static void MapPlaneToBytes(ReadOnlySpan<int> pels, int width, int height, int stride, Span<byte> dest, int targetBits, int scaleShift, int inputStart, int outputStart) {
            var maxVal = (1 << targetBits) - 1;
            var maxByte = (byte)Math.Min(maxVal, 255);

            var destOffset = 0;
            if (scaleShift >= 0) {
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = MapValueDownToByte(row[x], inputStart, scaleShift, outputStart, maxByte);
                    }
                }
            } else {
                var leftShift = -scaleShift;
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = MapValueUpToByte(row[x], inputStart, leftShift, outputStart, maxByte);
                    }
                }
            }
        }

        /// <summary>
        /// Fused mapping from Pel (int) buffer to ushort destination.
        /// </summary>
        public static void MapPlaneToUshorts(ReadOnlySpan<int> pels, int width, int height, int stride, Span<ushort> dest, int targetBits, int scaleShift, int inputStart, int outputStart) {
            var maxVal = (1 << targetBits) - 1;

            var destOffset = 0;
            if (scaleShift >= 0) {
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = (ushort)MapValueDown(row[x], inputStart, scaleShift, outputStart, maxVal);
                    }
                }
            } else {
                var leftShift = -scaleShift;
                for (var y = 0; y < height; y++) {
                    var row = pels.Slice(y * stride, width);
                    for (var x = 0; x < width; x++) {
                        dest[destOffset++] = (ushort)MapValueUp(row[x], inputStart, leftShift, outputStart, maxVal);
                    }
                }
            }
        }

        #endregion

        #region Downscale (scaleShift >= 0, right-shift)

        private static void MapPlaneDown(Span<int> pels, int width, int height, int stride, int shift, int inputStart, int outputStart, int maxVal) {
            var vecSize = Vector<int>.Count;
            for (var y = 0; y < height; y++) {
                var row = pels.Slice(y * stride, width);
                if (row.Length < vecSize) {
                    for (var x = 0; x < row.Length; x++) {
                        row[x] = MapValueDown(row[x], inputStart, shift, outputStart, maxVal);
                    }
                } else {
                    var vecInputStart = new Vector<int>(inputStart);
                    var vecOutputStart = new Vector<int>(outputStart);
                    var vecMax = new Vector<int>(maxVal);
                    var vecZero = Vector<int>.Zero;

                    var x = 0;
                    var alignedLen = row.Length - (row.Length % vecSize);
                    for (; x < alignedLen; x += vecSize) {
                        var slice = row.Slice(x, vecSize);
                        var vec = new Vector<int>(slice);
                        var sub = Vector.Subtract(vec, vecInputStart);
                        var underflow = Vector.LessThan(vec, vecInputStart);
                        sub = Vector.ConditionalSelect(underflow, vecZero, sub);
                        if (shift > 0) {
                            sub = Vector.ShiftRightLogical(sub, shift);
                        }
                        sub = Vector.Add(sub, vecOutputStart);
                        sub = Vector.Max(sub, vecZero);
                        sub = Vector.Min(sub, vecMax);
                        sub.CopyTo(slice);
                    }
                    for (; x < row.Length; x++) {
                        row[x] = MapValueDown(row[x], inputStart, shift, outputStart, maxVal);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapValueDown(int value, int inputStart, int shift, int outputStart, int maxVal) {
            if (value < inputStart) {
                return Math.Clamp(outputStart, 0, maxVal);
            }
            var mapped = ((value - inputStart) >> shift) + outputStart;
            return Math.Clamp(mapped, 0, maxVal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte MapValueDownToByte(int value, int inputStart, int shift, int outputStart, byte maxVal) {
            if (value < inputStart) {
                return (byte)Math.Clamp(outputStart, 0, maxVal);
            }
            var mapped = ((value - inputStart) >> shift) + outputStart;
            return (byte)Math.Clamp(mapped, 0, maxVal);
        }

        #endregion

        #region Upscale (scaleShift < 0, left-shift)

        private static void MapPlaneUp(Span<int> pels, int width, int height, int stride, int leftShift, int inputStart, int outputStart, int maxVal) {
            var vecSize = Vector<int>.Count;
            for (var y = 0; y < height; y++) {
                var row = pels.Slice(y * stride, width);
                if (row.Length < vecSize) {
                    for (var x = 0; x < row.Length; x++) {
                        row[x] = MapValueUp(row[x], inputStart, leftShift, outputStart, maxVal);
                    }
                } else {
                    var vecInputStart = new Vector<int>(inputStart);
                    var vecOutputStart = new Vector<int>(outputStart);
                    var vecMax = new Vector<int>(maxVal);
                    var vecZero = Vector<int>.Zero;

                    var x = 0;
                    var alignedLen = row.Length - (row.Length % vecSize);
                    for (; x < alignedLen; x += vecSize) {
                        var slice = row.Slice(x, vecSize);
                        var vec = new Vector<int>(slice);
                        var sub = Vector.Subtract(vec, vecInputStart);
                        var underflow = Vector.LessThan(vec, vecInputStart);
                        sub = Vector.ConditionalSelect(underflow, vecZero, sub);
                        sub = Vector.ShiftLeft(sub, leftShift);
                        sub = Vector.Add(sub, vecOutputStart);
                        sub = Vector.Max(sub, vecZero);
                        sub = Vector.Min(sub, vecMax);
                        sub.CopyTo(slice);
                    }
                    for (; x < row.Length; x++) {
                        row[x] = MapValueUp(row[x], inputStart, leftShift, outputStart, maxVal);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int MapValueUp(int value, int inputStart, int leftShift, int outputStart, int maxVal) {
            if (value < inputStart) {
                return Math.Clamp(outputStart, 0, maxVal);
            }
            var mapped = ((value - inputStart) << leftShift) + outputStart;
            return Math.Clamp(mapped, 0, maxVal);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte MapValueUpToByte(int value, int inputStart, int leftShift, int outputStart, byte maxVal) {
            if (value < inputStart) {
                return (byte)Math.Clamp(outputStart, 0, maxVal);
            }
            var mapped = ((value - inputStart) << leftShift) + outputStart;
            return (byte)Math.Clamp(mapped, 0, maxVal);
        }

        #endregion
    }
}
