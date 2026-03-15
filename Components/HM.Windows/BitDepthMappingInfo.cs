using System;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Public API for bit depth mapping parameter constraints and validation.
    /// Used by components for runtime validation and by WPF controls for slider ranges.
    /// </summary>
    public static class BitDepthMappingInfo {

        /// <summary>
        /// Validate mapping parameters. Throws if any parameter is out of range or inconsistent.
        /// </summary>
        public static void ValidateParameters(int sourceBits, int targetBits, int scaleShift, int windowStart) {
            if (sourceBits < 8 || sourceBits > 16) {
                throw new ArgumentOutOfRangeException(nameof(sourceBits), sourceBits, "Source bit depth must be between 8 and 16 (HEVC supported range).");
            }
            if (targetBits < 1 || targetBits > 16) {
                throw new ArgumentOutOfRangeException(nameof(targetBits), targetBits, "Target bit depth must be between 1 and 16.");
            }
            var windowBits = targetBits + scaleShift;
            if (windowBits < 0) {
                throw new ArgumentException($"targetBits + scaleShift = {windowBits} is negative. Window size would be fractional.", nameof(scaleShift));
            }
            if (windowBits > sourceBits) {
                throw new ArgumentException($"targetBits + scaleShift = {windowBits} exceeds sourceBits ({sourceBits}). Window cannot be larger than source range.", nameof(scaleShift));
            }
            if (windowStart < 0) {
                throw new ArgumentOutOfRangeException(nameof(windowStart), windowStart, "windowStart must be non-negative.");
            }
            if (scaleShift > 0 && (windowStart & ((1 << scaleShift) - 1)) != 0) {
                throw new ArgumentException($"windowStart ({windowStart}) must be aligned to 2^scaleShift ({1 << scaleShift}) when downscaling.", nameof(windowStart));
            }
            var windowSize = 1 << windowBits;
            var maxStart = (1 << sourceBits) - windowSize;
            if (windowStart > maxStart) {
                throw new ArgumentOutOfRangeException(nameof(windowStart), windowStart, $"windowStart must be at most {maxStart} for window size {windowSize} within {sourceBits}-bit range.");
            }
        }

        /// <summary>
        /// Compute the mapping window size: 2^(targetBits + scaleShift).
        /// Returns 0 if the resulting window bits would be negative or too large.
        /// </summary>
        public static int GetWindowSize(int targetBits, int scaleShift) {
            var windowBits = targetBits + scaleShift;
            if (windowBits < 0 || windowBits > 30) {
                return 0;
            }
            return 1 << windowBits;
        }

        /// <summary>
        /// Get the maximum valid windowStart for the given parameters.
        /// Returns 0 if the window is larger than or equal to the source range.
        /// </summary>
        public static int GetMaxWindowStart(int sourceBits, int targetBits, int scaleShift) {
            var windowSize = GetWindowSize(targetBits, scaleShift);
            if (windowSize <= 0) {
                return 0;
            }
            var maxStart = (1 << sourceBits) - windowSize;
            return Math.Max(maxStart, 0);
        }

        /// <summary>
        /// Get the required alignment step for windowStart.
        /// When scaleShift > 0 (downscaling), windowStart must be a multiple of 2^scaleShift.
        /// </summary>
        public static int GetWindowStartStep(int scaleShift) {
            return scaleShift > 0 ? 1 << scaleShift : 1;
        }

        /// <summary>
        /// Get the minimum valid scaleShift for the given target bit depth.
        /// </summary>
        public static int GetMinScaleShift(int targetBits) {
            return -targetBits;
        }

        /// <summary>
        /// Get the maximum valid scaleShift for the given source and target bit depths.
        /// </summary>
        public static int GetMaxScaleShift(int sourceBits, int targetBits) {
            return sourceBits - targetBits;
        }
    }
}
