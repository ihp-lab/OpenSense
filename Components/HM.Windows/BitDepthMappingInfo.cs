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
        public static void ValidateParameters(int sourceBits, int targetBits, int scaleShift, int inputStart, int outputStart) {
            if (sourceBits < 1 || sourceBits > 16) {
                throw new ArgumentOutOfRangeException(nameof(sourceBits), sourceBits, "Source bit depth must be between 1 and 16.");
            }
            if (targetBits < 1 || targetBits > 16) {
                throw new ArgumentOutOfRangeException(nameof(targetBits), targetBits, "Target bit depth must be between 1 and 16.");
            }
            if (inputStart < 0 || inputStart >= (1 << sourceBits)) {
                throw new ArgumentOutOfRangeException(nameof(inputStart), inputStart, $"inputStart must be in range [0, {(1 << sourceBits) - 1}].");
            }
            if (outputStart < 0 || outputStart >= (1 << targetBits)) {
                throw new ArgumentOutOfRangeException(nameof(outputStart), outputStart, $"outputStart must be in range [0, {(1 << targetBits) - 1}].");
            }
        }

        /// <summary>
        /// Get the maximum valid inputStart for the given source bit depth.
        /// </summary>
        public static int GetMaxInputStart(int sourceBits) {
            return (1 << sourceBits) - 1;
        }

        /// <summary>
        /// Get the maximum valid outputStart for the given target bit depth.
        /// </summary>
        public static int GetMaxOutputStart(int targetBits) {
            return (1 << targetBits) - 1;
        }

        /// <summary>
        /// Get the minimum valid scaleShift for the given target bit depth.
        /// </summary>
        public static int GetMinScaleShift(int targetBits) {
            return -targetBits;
        }

        /// <summary>
        /// Get the maximum valid scaleShift for the given source bit depth.
        /// </summary>
        public static int GetMaxScaleShift(int sourceBits) {
            return sourceBits - 1;
        }
    }
}
