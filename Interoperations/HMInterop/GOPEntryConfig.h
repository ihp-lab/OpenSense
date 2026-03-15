#pragma once

#include "Enums.h"

using namespace System;

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
}
