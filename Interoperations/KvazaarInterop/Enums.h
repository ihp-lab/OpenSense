#pragma once

using namespace System;

namespace KvazaarInterop {
    /// <summary>
    /// Chroma subsampling format (from kvz_chroma_format in kvazaar.h)
    /// </summary>
    public enum class ChromaFormat : int {
        Csp400 = 0,  // KVZ_CSP_400
        Csp420 = 1,  // KVZ_CSP_420
        Csp422 = 2,  // KVZ_CSP_422
        Csp444 = 3,  // KVZ_CSP_444
    };

    /// <summary>
    /// CU split termination mode (from kvz_cu_split_termination in kvazaar.h)
    /// </summary>
    public enum class CodingUnitSplitTermination : int {
        Zero = 0,  // KVZ_CU_SPLIT_TERMINATION_ZERO
        Off = 1,   // KVZ_CU_SPLIT_TERMINATION_OFF
    };

    /// <summary>
    /// Crypto features flags (from kvz_crypto_features in kvazaar.h)
    /// </summary>
    [Flags]
    public enum class CryptoFeatures : int {
        Off = 0,                      // KVZ_CRYPTO_OFF
        MotionVectors = (1 << 0),     // KVZ_CRYPTO_MVs
        MotionVectorSigns = (1 << 1), // KVZ_CRYPTO_MV_SIGNS
        TransformCoeffs = (1 << 2),   // KVZ_CRYPTO_TRANSF_COEFFS
        TransformCoeffSigns = (1 << 3), // KVZ_CRYPTO_TRANSF_COEFF_SIGNS
        IntraMode = (1 << 4),         // KVZ_CRYPTO_INTRA_MODE
        On = (1 << 5) - 1,            // KVZ_CRYPTO_ON
    };

    /// <summary>
    /// File format (from kvz_file_format in kvazaar.h)
    /// </summary>
    public enum class FileFormat : int {
        Auto = 0,  // KVZ_FORMAT_AUTO
        Y4M = 1,   // KVZ_FORMAT_Y4M
        YUV = 2,   // KVZ_FORMAT_YUV
    };

    /// <summary>
    /// Hash algorithm (from kvz_hash in kvazaar.h)
    /// </summary>
    public enum class HashAlgorithm : int {
        None = 0,     // KVZ_HASH_NONE
        Checksum = 1, // KVZ_HASH_CHECKSUM
        MD5 = 2,      // KVZ_HASH_MD5
    };

    /// <summary>
    /// Input format (from kvz_input_format in kvazaar.h)
    /// </summary>
    public enum class InputFormat : int {
        P400 = 0,  // KVZ_FORMAT_P400
        P420 = 1,  // KVZ_FORMAT_P420
        P422 = 2,  // KVZ_FORMAT_P422
        P444 = 3,  // KVZ_FORMAT_P444
    };

    /// <summary>
    /// Integer motion estimation algorithm (from kvz_ime_algorithm in kvazaar.h)
    /// </summary>
    public enum class IntegerMotionEstimationAlgorithm : int {
        HexagonBasedSearch = 0,  // KVZ_IME_HEXBS
        TestZoneSearch = 1,      // KVZ_IME_TZ
        FullSearch = 2,          // KVZ_IME_FULL
        FullSearch8 = 3,         // KVZ_IME_FULL8
        FullSearch16 = 4,        // KVZ_IME_FULL16
        FullSearch32 = 5,        // KVZ_IME_FULL32
        FullSearch64 = 6,        // KVZ_IME_FULL64
        DiamondSearch = 7,       // KVZ_IME_DIA
    };

    /// <summary>
    /// Interlacing method (from kvz_interlacing in kvazaar.h)
    /// </summary>
    public enum class Interlacing : int {
        None = 0,            // KVZ_INTERLACING_NONE
        TopFieldFirst = 1,   // KVZ_INTERLACING_TFF
        BottomFieldFirst = 2, // KVZ_INTERLACING_BFF
    };

    /// <summary>
    /// Motion estimation early termination (from kvz_me_early_termination in kvazaar.h)
    /// </summary>
    public enum class MotionEstimationEarlyTermination : int {
        Off = 0,       // KVZ_ME_EARLY_TERMINATION_OFF
        On = 1,        // KVZ_ME_EARLY_TERMINATION_ON
        Sensitive = 2, // KVZ_ME_EARLY_TERMINATION_SENSITIVE
    };

    /// <summary>
    /// Motion vector constraint (from kvz_mv_constraint in kvazaar.h)
    /// </summary>
    public enum class MotionVectorConstraint : int {
        None = 0,              // KVZ_MV_CONSTRAIN_NONE
        Frame = 1,             // KVZ_MV_CONSTRAIN_FRAME
        Tile = 2,              // KVZ_MV_CONSTRAIN_TILE
        FrameAndTile = 3,      // KVZ_MV_CONSTRAIN_FRAME_AND_TILE
        FrameAndTileMargin = 4, // KVZ_MV_CONSTRAIN_FRAME_AND_TILE_MARGIN
    };

    /// <summary>
    /// NAL unit type (from kvz_nal_unit_type in kvazaar.h)
    /// </summary>
    public enum class NetworkAbstractionLayerUnitType : int {
        TrailN = 0,      // KVZ_NAL_TRAIL_N
        TrailR = 1,      // KVZ_NAL_TRAIL_R
        TsaN = 2,        // KVZ_NAL_TSA_N
        TsaR = 3,        // KVZ_NAL_TSA_R
        StsaN = 4,       // KVZ_NAL_STSA_N
        StsaR = 5,       // KVZ_NAL_STSA_R
        RadlN = 6,       // KVZ_NAL_RADL_N
        RadlR = 7,       // KVZ_NAL_RADL_R
        RaslN = 8,       // KVZ_NAL_RASL_N
        RaslR = 9,       // KVZ_NAL_RASL_R
        BlaWLp = 16,     // KVZ_NAL_BLA_W_LP
        BlaWRadl = 17,   // KVZ_NAL_BLA_W_RADL
        BlaNLp = 18,     // KVZ_NAL_BLA_N_LP
        IdrWRadl = 19,   // KVZ_NAL_IDR_W_RADL
        IdrNLp = 20,     // KVZ_NAL_IDR_N_LP
        CraNut = 21,     // KVZ_NAL_CRA_NUT
        RsvIrapVcl22 = 22, // KVZ_NAL_RSV_IRAP_VCL22
        RsvIrapVcl23 = 23, // KVZ_NAL_RSV_IRAP_VCL23
        VpsNut = 32,     // KVZ_NAL_VPS_NUT
        SpsNut = 33,     // KVZ_NAL_SPS_NUT
        PpsNut = 34,     // KVZ_NAL_PPS_NUT
        AudNut = 35,     // KVZ_NAL_AUD_NUT
        EosNut = 36,     // KVZ_NAL_EOS_NUT
        EobNut = 37,     // KVZ_NAL_EOB_NUT
        FdNut = 38,      // KVZ_NAL_FD_NUT
        PrefixSeiNut = 39, // KVZ_NAL_PREFIX_SEI_NUT
        SuffixSeiNut = 40, // KVZ_NAL_SUFFIX_SEI_NUT
    };

    /// <summary>
    /// Rate control algorithm (from kvz_rc_algorithm in kvazaar.h)
    /// </summary>
    public enum class RateControlAlgorithm : int {
        NoRateControl = 0,  // KVZ_NO_RC
        Lambda = 1,         // KVZ_LAMBDA
        OBA = 2,            // KVZ_OBA
    };

    /// <summary>
    /// ROI format (from kvz_roi_format in kvazaar.h)
    /// </summary>
    public enum class RoiFormat : int {
        Text = 0,   // KVZ_ROI_TXT
        Binary = 1, // KVZ_ROI_BIN
    };

    /// <summary>
    /// Sample adaptive offset type (from kvz_sao in kvazaar.h)
    /// </summary>
    public enum class SampleAdaptiveOffset : int {
        Off = 0,   // KVZ_SAO_OFF
        Edge = 1,  // KVZ_SAO_EDGE
        Band = 2,  // KVZ_SAO_BAND
        Full = 3,  // KVZ_SAO_FULL
    };

    /// <summary>
    /// Scaling list type (from kvz_scalinglist in kvazaar.h)
    /// </summary>
    public enum class ScalingList : int {
        Off = 0,     // KVZ_SCALING_LIST_OFF
        Custom = 1,  // KVZ_SCALING_LIST_CUSTOM
        Default = 2, // KVZ_SCALING_LIST_DEFAULT
    };

    /// <summary>
    /// Slice mode (from kvz_slices in kvazaar.h)
    /// </summary>
    [Flags]
    public enum class SliceMode : int {
        None = 0,           // KVZ_SLICES_NONE
        Tiles = (1 << 0),   // KVZ_SLICES_TILES
        WPP = (1 << 1),     // KVZ_SLICES_WPP
    };

    /// <summary>
    /// Slice type (from kvz_slice_type in kvazaar.h)
    /// </summary>
    public enum class SliceType : int {
        B = 0,  // KVZ_SLICE_B
        P = 1,  // KVZ_SLICE_P
        I = 2,  // KVZ_SLICE_I
    };
}