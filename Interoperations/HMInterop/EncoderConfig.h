#pragma once

#include "GOPEntryConfig.h"

using namespace System;

class TEncTop;

namespace HMInterop {
    /// <summary>
    /// Encoder configuration for HM HEVC encoder.
    /// Records configuration values that are applied to TEncTop before encoding starts.
    /// </summary>
    [Serializable]
    public ref class EncoderConfig {
    public:
        EncoderConfig();

#pragma region Basic
        /// <summary>
        /// Source width in pixels. Must be set before encoding.
        /// </summary>
        property int SourceWidth;

        /// <summary>
        /// Source height in pixels. Must be set before encoding.
        /// </summary>
        property int SourceHeight;

        /// <summary>
        /// Frame rate. Must be > 0 for HM. Default: 30.
        /// </summary>
        property int FrameRate;

        /// <summary>
        /// Chroma format.
        /// </summary>
        // Not Nullable: C++/CLI System::Nullable does not serialize correctly in JSON.
        // Auto-detection logic is handled in the C# FileWriterConfiguration layer.
        property ChromaFormat ChromaFormatIdc;
#pragma endregion

#pragma region BitDepth
        /// <summary>
        /// Input bit depth for luma channel.
        /// </summary>
        // Not Nullable: C++/CLI System::Nullable does not serialize correctly in JSON.
        // Auto-detection logic is handled in the C# FileWriterConfiguration layer.
        property int InputBitDepth;

        /// <summary>
        /// Internal bit depth for encoding.
        /// </summary>
        // Not Nullable: C++/CLI System::Nullable does not serialize correctly in JSON.
        // "Same as input" logic is handled in the C# FileWriterConfiguration layer.
        property int InternalBitDepth;
#pragma endregion

#pragma region CodingStructure
        /// <summary>
        /// Intra period in frames. 1 = every frame is intra (All-Intra). Default: 32.
        /// Must be a multiple of GOPSize for hierarchical B-frame structures.
        /// </summary>
        property int IntraPeriod;

        /// <summary>
        /// Decoding refresh type. Default: IDR.
        /// </summary>
        property DecodingRefreshType DecodingRefreshType;

        /// <summary>
        /// GOP entry configuration array. Length determines GOPSize.
        /// Default: 8-entry hierarchical B-frame structure for good compression.
        /// </summary>
        property cli::array<GOPEntryConfig^>^ GOPEntries;
#pragma endregion

#pragma region QualityControl
        /// <summary>
        /// Quantization parameter. Default: 32.
        /// </summary>
        property int QP;

        /// <summary>
        /// Enable lossless coding (transquant bypass). Default: false.
        /// </summary>
        property bool Lossless;
#pragma endregion

#pragma region Profile
        /// <summary>
        /// HEVC profile. Default: MainRExt (required for high bit depth).
        /// </summary>
        property ProfileName Profile;

        /// <summary>
        /// Bit depth constraint value. Default: 16.
        /// </summary>
        property int BitDepthConstraintValue;
#pragma endregion

#pragma region CUStructure
        /// <summary>
        /// Maximum CU width. Default: 64.
        /// </summary>
        property int MaxCUWidth;

        /// <summary>
        /// Maximum CU height. Default: 64.
        /// </summary>
        property int MaxCUHeight;

        /// <summary>
        /// Maximum total CU depth (partition depth). Default: 4.
        /// </summary>
        property int MaxTotalCUDepth;
#pragma endregion

#pragma region Filter
        /// <summary>
        /// Disable loop filter (deblocking). Default: false.
        /// </summary>
        property bool LoopFilterDisable;

        /// <summary>
        /// Enable SAO (Sample Adaptive Offset). Default: true.
        /// </summary>
        property bool UseSAO;
#pragma endregion

#pragma region MotionEstimation
        /// <summary>
        /// Motion estimation search range in pixels. Default: 64.
        /// Only used for inter prediction (GOPSize > 1).
        /// </summary>
        property int SearchRange;

        /// <summary>
        /// Bi-prediction search range in pixels. Default: 4.
        /// Only used for inter prediction (GOPSize > 1).
        /// </summary>
        property int BipredSearchRange;

        /// <summary>
        /// Motion estimation search method. Default: Diamond.
        /// Only used for inter prediction (GOPSize > 1).
        /// </summary>
        property MotionSearchMethod MotionEstimationSearchMethod;
#pragma endregion

#pragma region RangeExtension
        /// <summary>
        /// Enable extended precision processing for high bit depth. Default: true.
        /// </summary>
        property bool ExtendedPrecision;

        /// <summary>
        /// Enable high precision offsets. Default: true.
        /// </summary>
        property bool HighPrecisionOffsets;
#pragma endregion

#pragma region Verification
        /// <summary>
        /// Decoded picture hash type for verification. Default: None (disabled).
        /// </summary>
        property HashType DecodedPictureHashSEIType;
#pragma endregion

#pragma region RateControl
        /// <summary>
        /// Enable rate control. Default: false.
        /// Conflicts with VFR streaming (assumes fixed timing).
        /// </summary>
        property bool UseRateCtrl;

        /// <summary>
        /// Target bitrate in bits per second. Only used when UseRateCtrl is true. Default: 0.
        /// </summary>
        property int TargetBitrate;
#pragma endregion

        EncoderConfig^ Clone();

    internal:
        /// <summary>
        /// Apply all configuration values to the TEncTop encoder instance.
        /// Called internally by Encoder during construction.
        /// </summary>
        void ApplyTo(TEncTop* encoder);

    };
}
