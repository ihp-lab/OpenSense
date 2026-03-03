#pragma once

#include "Enums.h"

using namespace System;

class TEncTop;

namespace HMInterop {
    /// <summary>
    /// Configuration for a single GOP entry.
    /// Maps to native GOPEntry struct in TEncCfg.h.
    /// </summary>
    [Serializable]
    public ref class GOPEntryConfig {
    public:
        GOPEntryConfig();

#pragma region Basic
        /// <summary>
        /// Picture Order Count within the GOP.
        /// </summary>
        property int POC;

        /// <summary>
        /// Slice type (B, P, or I).
        /// </summary>
        property SliceType SliceType;

        /// <summary>
        /// Temporal layer ID. Higher layers can be dropped without breaking decoding.
        /// </summary>
        property int TemporalId;

        /// <summary>
        /// Whether this picture is used as a reference by other pictures.
        /// </summary>
        property bool IsReferencePicture;
#pragma endregion

#pragma region Quality
        /// <summary>
        /// QP offset relative to base QP.
        /// </summary>
        property int QPOffset;

        /// <summary>
        /// Lambda factor for rate-distortion optimization.
        /// </summary>
        property double QPFactor;

        /// <summary>
        /// QP offset model offset (for QP adaptation model).
        /// </summary>
        property double QPOffsetModelOffset;

        /// <summary>
        /// QP offset model scale (for QP adaptation model).
        /// </summary>
        property double QPOffsetModelScale;

        /// <summary>
        /// Cb chroma QP offset.
        /// </summary>
        property int CbQPOffset;

        /// <summary>
        /// Cr chroma QP offset.
        /// </summary>
        property int CrQPOffset;
#pragma endregion

#pragma region ReferencePictures
        /// <summary>
        /// Number of active reference pictures used for prediction.
        /// </summary>
        property int NumRefPicsActive;

        /// <summary>
        /// Total number of reference pictures in the reference picture set.
        /// </summary>
        property int NumRefPics;

        /// <summary>
        /// POC offsets of reference pictures (relative to this picture's POC).
        /// Array of length 16 (MAX_NUM_REF_PICS). Only the first NumRefPics entries are used.
        /// </summary>
        property cli::array<int>^ ReferencePics;

        /// <summary>
        /// Flags indicating which reference pictures are used by the current picture.
        /// Array of length 16 (MAX_NUM_REF_PICS). Only the first NumRefPics entries are used.
        /// </summary>
        property cli::array<int>^ UsedByCurrPic;
#pragma endregion

#pragma region LoopFilter
        /// <summary>
        /// Deblocking filter beta offset divided by 2.
        /// </summary>
        property int BetaOffsetDiv2;

        /// <summary>
        /// Deblocking filter tc offset divided by 2.
        /// </summary>
        property int TcOffsetDiv2;
#pragma endregion

#pragma region RPSPrediction
        /// <summary>
        /// Inter RPS prediction mode. 0 = explicit, 1 = delta prediction.
        /// </summary>
        property int InterRPSPrediction;

        /// <summary>
        /// Delta RPS value for inter RPS prediction.
        /// </summary>
        property int DeltaRPS;

        /// <summary>
        /// Number of reference IDC entries.
        /// </summary>
        property int NumRefIdc;

        /// <summary>
        /// Reference IDC values for inter RPS prediction.
        /// Array of length 17 (MAX_NUM_REF_PICS + 1). Only the first NumRefIdc entries are used.
        /// </summary>
        property cli::array<int>^ RefIdc;
#pragma endregion

#pragma region TextHelpers
        /// <summary>
        /// Comma-separated string representation of ReferencePics for WPF binding.
        /// Setting this property also updates NumRefPics.
        /// </summary>
        property String^ ReferencePicsText {
            String^ get();
            void set(String^ value);
        }

        /// <summary>
        /// Comma-separated string representation of UsedByCurrPic for WPF binding.
        /// </summary>
        property String^ UsedByCurrPicText {
            String^ get();
            void set(String^ value);
        }

        /// <summary>
        /// Comma-separated string representation of RefIdc for WPF binding.
        /// Setting this property also updates NumRefIdc.
        /// </summary>
        property String^ RefIdcText {
            String^ get();
            void set(String^ value);
        }
#pragma endregion

        GOPEntryConfig^ Clone();
    };

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
        /// Chroma format. Default: Chroma400 (monochrome).
        /// </summary>
        property ChromaFormat ChromaFormatIdc;
#pragma endregion

#pragma region BitDepth
        /// <summary>
        /// Input bit depth for luma channel. Default: 16.
        /// </summary>
        property int InputBitDepth;

        /// <summary>
        /// Internal bit depth for encoding. Default: 16.
        /// </summary>
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
