#include "pch.h"

#pragma managed(push, off)
#include <vector>
#include "TLibCommon/CommonDef.h"
#include "TLibEncoder/TEncTop.h"
#pragma managed(pop)

#include "EncoderConfig.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Text;

namespace HMInterop {

    // ======================================================================
    // GOPEntryConfig
    // ======================================================================

    GOPEntryConfig::GOPEntryConfig() {
        POC = 1;
        SliceType = HMInterop::SliceType::B;
        TemporalId = 0;
        IsReferencePicture = true;
        QPOffset = 1;
        QPFactor = 0.4624;
        QPOffsetModelOffset = 0.0;
        QPOffsetModelScale = 0.0;
        CbQPOffset = 0;
        CrQPOffset = 0;
        NumRefPicsActive = 1;
        NumRefPics = 0;
        ReferencePics = gcnew cli::array<int>(MAX_NUM_REF_PICS);
        UsedByCurrPic = gcnew cli::array<int>(MAX_NUM_REF_PICS);
        BetaOffsetDiv2 = 0;
        TcOffsetDiv2 = 0;
        InterRPSPrediction = 0;
        DeltaRPS = 0;
        NumRefIdc = 0;
        RefIdc = gcnew cli::array<int>(MAX_NUM_REF_PICS + 1);
    }

    GOPEntryConfig^ GOPEntryConfig::Clone() {
        auto clone = safe_cast<GOPEntryConfig^>(MemberwiseClone());
        if (ReferencePics != nullptr) {
            clone->ReferencePics = safe_cast<cli::array<int>^>(ReferencePics->Clone());
        }
        if (UsedByCurrPic != nullptr) {
            clone->UsedByCurrPic = safe_cast<cli::array<int>^>(UsedByCurrPic->Clone());
        }
        if (RefIdc != nullptr) {
            clone->RefIdc = safe_cast<cli::array<int>^>(RefIdc->Clone());
        }
        return clone;
    }

    // ---- Text helper properties ----

    static String^ ArrayToText(cli::array<int>^ arr, int count) {
        if (arr == nullptr || count <= 0) {
            return "";
        }
        auto sb = gcnew StringBuilder();
        auto limit = Math::Min(count, arr->Length);
        for (auto i = 0; i < limit; i++) {
            if (i > 0) {
                sb->Append(", ");
            }
            sb->Append(arr[i]);
        }
        return sb->ToString();
    }

    static int TextToArray(String^ text, cli::array<int>^ arr) {
        if (arr == nullptr || String::IsNullOrWhiteSpace(text)) {
            return 0;
        }
        auto parts = text->Split(',');
        int count = 0;
        for (auto i = 0; i < parts->Length && count < arr->Length; i++) {
            auto trimmed = parts[i]->Trim();
            int val;
            if (Int32::TryParse(trimmed, val)) {
                arr[count] = val;
                count++;
            }
        }
        return count;
    }

    String^ GOPEntryConfig::ReferencePicsText::get() {
        return ArrayToText(ReferencePics, NumRefPics);
    }

    void GOPEntryConfig::ReferencePicsText::set(String^ value) {
        if (ReferencePics == nullptr) {
            ReferencePics = gcnew cli::array<int>(MAX_NUM_REF_PICS);
        }
        NumRefPics = TextToArray(value, ReferencePics);
    }

    String^ GOPEntryConfig::UsedByCurrPicText::get() {
        return ArrayToText(UsedByCurrPic, NumRefPics);
    }

    void GOPEntryConfig::UsedByCurrPicText::set(String^ value) {
        if (UsedByCurrPic == nullptr) {
            UsedByCurrPic = gcnew cli::array<int>(MAX_NUM_REF_PICS);
        }
        TextToArray(value, UsedByCurrPic);
    }

    String^ GOPEntryConfig::RefIdcText::get() {
        return ArrayToText(RefIdc, NumRefIdc);
    }

    void GOPEntryConfig::RefIdcText::set(String^ value) {
        if (RefIdc == nullptr) {
            RefIdc = gcnew cli::array<int>(MAX_NUM_REF_PICS + 1);
        }
        NumRefIdc = TextToArray(value, RefIdc);
    }

    // ======================================================================
    // EncoderConfig
    // ======================================================================

    static GOPEntryConfig^ MakeGOPEntry(int poc, HMInterop::SliceType type, int tid, bool refPic, int qpOff, double qpFact, int numRefActive, cli::array<int>^ refs, cli::array<int>^ used) {
        auto e = gcnew GOPEntryConfig();
        e->POC = poc;
        e->SliceType = type;
        e->TemporalId = tid;
        e->IsReferencePicture = refPic;
        e->QPOffset = qpOff;
        e->QPFactor = qpFact;
        e->NumRefPicsActive = numRefActive;
        e->NumRefPics = refs->Length;
        for (auto i = 0; i < refs->Length && i < e->ReferencePics->Length; i++) {
            e->ReferencePics[i] = refs[i];
            e->UsedByCurrPic[i] = (i < used->Length) ? used[i] : 1;
        }
        return e;
    }

    EncoderConfig::EncoderConfig() {
        // Basic
        SourceWidth = 0;
        SourceHeight = 0;
        FrameRate = 30;
        ChromaFormatIdc = ChromaFormat::Chroma420;

        InputBitDepth = 8;
        InternalBitDepth = 8;

        // Coding Structure
        IntraPeriod = 32;
        DecodingRefreshType = HMInterop::DecodingRefreshType::CRA;

        // Default GOP: Hierarchical B, GOPSize=8, 4 temporal layers
        // Matches HM reference encoder_randomaccess_main_GOP8.cfg.
        // Entry 0 uses cross-GOP references for better compression.
        // Extra RPS entries are created in ApplyTo to handle the first GOP
        // where some cross-GOP references point to frames before the sequence start.
        {
            auto B = HMInterop::SliceType::B;
            GOPEntries = gcnew cli::array<GOPEntryConfig^>(8);
            GOPEntries[0] = MakeGOPEntry(8, B, 0, true,  1, 0.442,  2, gcnew cli::array<int>{ -8, -12, -16 }, gcnew cli::array<int>{ 1, 0, 1 });
            GOPEntries[1] = MakeGOPEntry(4, B, 1, true,  2, 0.3536, 2, gcnew cli::array<int>{ -4, -8, 4 },    gcnew cli::array<int>{ 1, 1, 1 });
            GOPEntries[2] = MakeGOPEntry(2, B, 2, true,  3, 0.3536, 2, gcnew cli::array<int>{ -2, -6, 2, 6 }, gcnew cli::array<int>{ 1, 1, 1, 1 });
            GOPEntries[3] = MakeGOPEntry(1, B, 3, false, 4, 0.68,   2, gcnew cli::array<int>{ -1, 1, 3, 7 },  gcnew cli::array<int>{ 1, 1, 1, 1 });
            GOPEntries[4] = MakeGOPEntry(3, B, 3, false, 4, 0.68,   2, gcnew cli::array<int>{ -1, -3, 1, 5 }, gcnew cli::array<int>{ 1, 1, 1, 1 });
            GOPEntries[5] = MakeGOPEntry(6, B, 2, true,  3, 0.3536, 2, gcnew cli::array<int>{ -2, -6, 2 },    gcnew cli::array<int>{ 1, 1, 1 });
            GOPEntries[6] = MakeGOPEntry(5, B, 3, false, 4, 0.68,   2, gcnew cli::array<int>{ -1, -5, 1, 3 }, gcnew cli::array<int>{ 1, 1, 1, 1 });
            GOPEntries[7] = MakeGOPEntry(7, B, 3, false, 4, 0.68,   2, gcnew cli::array<int>{ -1, -3, -7, 1 },gcnew cli::array<int>{ 1, 1, 1, 1 });
        }

        // Quality Control
        QP = 32;
        Lossless = false;

        // Profile
        Profile = ProfileName::MainRExt;
        BitDepthConstraintValue = 16;

        // CU Structure
        MaxCUWidth = 64;
        MaxCUHeight = 64;
        MaxTotalCUDepth = 4;

        // Motion Estimation
        SearchRange = 64;
        BipredSearchRange = 4;
        MotionEstimationSearchMethod = MotionSearchMethod::Diamond;

        // Filter
        LoopFilterDisable = false;
        UseSAO = true;

        // Range Extension
        ExtendedPrecision = true;
        HighPrecisionOffsets = true;

        // Verification
        DecodedPictureHashSEIType = HashType::None;

        // Rate Control
        UseRateCtrl = false;
        TargetBitrate = 0;
    }

    EncoderConfig^ EncoderConfig::Clone() {
        auto clone = safe_cast<EncoderConfig^>(MemberwiseClone());
        if (GOPEntries != nullptr) {
            clone->GOPEntries = gcnew cli::array<GOPEntryConfig^>(GOPEntries->Length);
            for (auto i = 0; i < GOPEntries->Length; i++) {
                if (GOPEntries[i] != nullptr) {
                    clone->GOPEntries[i] = GOPEntries[i]->Clone();
                }
            }
        }
        return clone;
    }

    static bool IsPowerOfTwo(int value) {
        return value > 0 && (value & (value - 1)) == 0;
    }

    static int Log2(int value) {
        auto result = 0;
        while (value > 1) {
            value >>= 1;
            result++;
        }
        return result;
    }

    void EncoderConfig::ApplyTo(TEncTop* encoder) {
        if (!encoder) {
            throw gcnew ArgumentNullException("encoder");
        }
        if (SourceWidth <= 0 || SourceHeight <= 0) {
            throw gcnew System::InvalidOperationException("SourceWidth and SourceHeight must be set before encoding.");
        }

        // ---- Parameter Validation ----

        if (FrameRate <= 0) {
            throw gcnew ArgumentException("FrameRate must be greater than 0.");
        }

        if (InputBitDepth < 8 || InputBitDepth > 16) {
            throw gcnew ArgumentException(String::Format("InputBitDepth must be between 8 and 16, got {0}.", InputBitDepth));
        }

        if (InternalBitDepth < 8 || InternalBitDepth > 16) {
            throw gcnew ArgumentException(String::Format("InternalBitDepth must be between 8 and 16, got {0}.", InternalBitDepth));
        }

        if (QP < 0 || QP > 51) {
            throw gcnew ArgumentException(String::Format("QP must be between 0 and 51, got {0}.", QP));
        }

        if (!IsPowerOfTwo(MaxCUWidth) || !IsPowerOfTwo(MaxCUHeight)) {
            throw gcnew ArgumentException(String::Format("MaxCUWidth ({0}) and MaxCUHeight ({1}) must be powers of 2.", MaxCUWidth, MaxCUHeight));
        }

        if (MaxCUWidth < 16 || MaxCUWidth > 64) {
            throw gcnew ArgumentException(String::Format("MaxCUWidth must be between 16 and 64, got {0}.", MaxCUWidth));
        }

        if (MaxCUHeight < 16 || MaxCUHeight > 64) {
            throw gcnew ArgumentException(String::Format("MaxCUHeight must be between 16 and 64, got {0}.", MaxCUHeight));
        }

        if (MaxTotalCUDepth < 1) {
            throw gcnew ArgumentException(String::Format("MaxTotalCUDepth must be at least 1, got {0}.", MaxTotalCUDepth));
        }

        {
            // Minimum CU size = MaxCUWidth >> MaxTotalCUDepth
            // Must be >= 4 (minimum TU size is 4x4)
            auto minCUSize = MaxCUWidth >> MaxTotalCUDepth;
            if (minCUSize < 4) {
                auto maxAllowedDepth = Log2(MaxCUWidth) - 2;
                throw gcnew ArgumentException(String::Format("MaxTotalCUDepth ({0}) is too large for MaxCUWidth ({1}). Minimum CU size would be {2}x{2}, but must be at least 4x4. Maximum allowed depth for this CU width is {3}.", MaxTotalCUDepth, MaxCUWidth, minCUSize, maxAllowedDepth));
            }
        }

        // Derive GOP size from GOPEntries array length
        auto gopSize = (GOPEntries != nullptr && GOPEntries->Length > 0) ? GOPEntries->Length : 1;

        if (IntraPeriod > 1 && gopSize > 1 && IntraPeriod % gopSize != 0) {
            throw gcnew ArgumentException(String::Format("IntraPeriod ({0}) must be a multiple of GOPSize ({1}) for hierarchical B-frame structures.", IntraPeriod, gopSize));
        }

        // Validate GOP entry references: every reference must point to a valid
        // POC within the GOP or cross-GOP range, and the reference structure must
        // be self-consistent.  This catches stale serialized configurations early
        // instead of letting HM hit an assertion deep in encoding.
        if (GOPEntries != nullptr && gopSize > 1) {
            // Collect the set of POC values defined by all GOP entries
            auto pocSet = gcnew HashSet<int>();
            for (auto i = 0; i < GOPEntries->Length; i++) {
                if (GOPEntries[i] != nullptr) {
                    pocSet->Add(GOPEntries[i]->POC);
                }
            }
            for (auto i = 0; i < GOPEntries->Length; i++) {
                auto entry = GOPEntries[i];
                if (entry == nullptr || entry->ReferencePics == nullptr) {
                    continue;
                }
                for (auto j = 0; j < entry->NumRefPics && j < entry->ReferencePics->Length; j++) {
                    auto refDelta = entry->ReferencePics[j];
                    auto refPOC = entry->POC + refDelta;
                    // Reference must point to POC 0 (the IRAP anchor) or
                    // a POC defined by another GOP entry (mod GOPSize for cross-GOP)
                    auto modPOC = refPOC % gopSize;
                    if (modPOC < 0) {
                        modPOC += gopSize;
                    }
                    // modPOC == 0 means referencing the GOP anchor (POC 0, gopSize, 2*gopSize, etc.)
                    if (modPOC != 0 && !pocSet->Contains(modPOC)) {
                        throw gcnew ArgumentException(String::Format("GOPEntry[{0}] (POC {1}): reference delta {2} points to absolute POC {3}, which does not correspond to any GOP entry (POC mod {4} = {5}). Check that the GOP reference structure is correct.", i, entry->POC, refDelta, refPOC, gopSize, modPOC));
                    }
                }
            }
        }

        auto internalBitDepth = InternalBitDepth;

        // Compute maxTLayers and maxRefDist from GOP entries
        auto maxTLayers = 1;
        auto maxRefDist = gopSize;
        if (GOPEntries != nullptr) {
            for (auto i = 0; i < GOPEntries->Length; i++) {
                if (GOPEntries[i] == nullptr) {
                    continue;
                }
                if (GOPEntries[i]->TemporalId + 1 > maxTLayers) {
                    maxTLayers = GOPEntries[i]->TemporalId + 1;
                }
                if (GOPEntries[i]->ReferencePics != nullptr) {
                    for (auto j = 0; j < GOPEntries[i]->NumRefPics && j < GOPEntries[i]->ReferencePics->Length; j++) {
                        auto dist = GOPEntries[i]->ReferencePics[j] < 0 ? -GOPEntries[i]->ReferencePics[j] : GOPEntries[i]->ReferencePics[j];
                        if (dist > maxRefDist) {
                            maxRefDist = dist;
                        }
                    }
                }
            }
        }
        auto maxDecPicBuf = (gopSize == 1) ? 1 : ((maxRefDist + 1 > gopSize + 1) ? maxRefDist + 1 : gopSize + 1);

        // ==================================================================
        // Setter order follows reference encoder app TAppEncTop::xInitLibCfg
        // (TAppEncTop.cpp lines 74-605).
        // ==================================================================

        // ---- VPS ----
        {
            TComVPS vps;
            vps.setMaxTLayers(maxTLayers);
            vps.setTemporalNestingFlag(maxTLayers > 1);
            vps.setMaxLayers(1);
            for (auto i = 0; i < MAX_TLAYER; i++) {
                if (gopSize == 1) {
                    vps.setNumReorderPics(0, i);
                    vps.setMaxDecPicBuffering(1, i);
                } else {
                    vps.setNumReorderPics(gopSize - 1, i);
                    vps.setMaxDecPicBuffering(maxDecPicBuf, i);
                }
            }
            encoder->setVPS(&vps);
        }

        encoder->setCabacZeroWordPaddingEnabled(true);

        // ---- Profile and Level ----
        encoder->setProfile(static_cast<::Profile::Name>(Profile));
        encoder->setLevel(Level::MAIN, Level::NONE);
        encoder->setBitDepthConstraintValue(BitDepthConstraintValue);
        encoder->setChromaFormatConstraintValue(static_cast<::ChromaFormat>(ChromaFormatIdc));
        encoder->setIntraConstraintFlag(gopSize == 1 && IntraPeriod == 1);
        encoder->setOnePictureOnlyConstraintFlag(false);
        encoder->setLowerBitRateConstraintFlag(true);
        encoder->setProgressiveSourceFlag(true);
        encoder->setInterlacedSourceFlag(false);
        encoder->setNonPackedConstraintFlag(false);
        encoder->setFrameOnlyConstraintFlag(true);

        // ---- Source ----
        encoder->setSourceWidth(SourceWidth);
        encoder->setSourceHeight(SourceHeight);
        encoder->setConformanceWindow(0, 0, 0, 0);
        encoder->setFrameRate(FrameRate);
        encoder->setFramesToBeEncoded(INT_MAX);
        encoder->setFrameSkip(0);
        encoder->setTemporalSubsampleRatio(1);
        {
            Int pad[2] = { 0, 0 };
            encoder->setSourcePadding(pad);
        }

        // ---- Coding Structure ----
        encoder->setIntraPeriod(IntraPeriod);
        encoder->setDecodingRefreshType(static_cast<Int>(DecodingRefreshType));
        encoder->setReWriteParamSetsFlag(false);
        encoder->setGOPSize(gopSize);

        // GOP list - convert managed GOPEntries to native GOPEntry array
        auto extraRPSCount = 0;
        {
            std::vector<GOPEntry> gopList(MAX_GOP);
            if (gopSize == 1 && (GOPEntries == nullptr || GOPEntries->Length == 0)) {
                // All-Intra fallback
                gopList[0].m_POC = 1;
                gopList[0].m_QPFactor = 1.0;
                gopList[0].m_numRefPicsActive = 1;
                gopList[0].m_numRefPics = 0;
                gopList[0].m_betaOffsetDiv2 = 0;
                gopList[0].m_tcOffsetDiv2 = 0;
            } else if (GOPEntries != nullptr) {
                for (auto i = 0; i < GOPEntries->Length && i < MAX_GOP; i++) {
                    auto src = GOPEntries[i];
                    if (src == nullptr) {
                        continue;
                    }
                    auto& dst = gopList[i];
                    dst.m_POC = src->POC;
                    // Convert SliceType enum to char
                    switch (src->SliceType) {
                        case HMInterop::SliceType::B: dst.m_sliceType = 'B'; break;
                        case HMInterop::SliceType::P: dst.m_sliceType = 'P'; break;
                        case HMInterop::SliceType::I: dst.m_sliceType = 'I'; break;
                        default: dst.m_sliceType = 'B'; break;
                    }
                    dst.m_temporalId = src->TemporalId;
                    dst.m_refPic = src->IsReferencePicture;
                    dst.m_QPOffset = src->QPOffset;
                    dst.m_QPFactor = src->QPFactor;
                    dst.m_QPOffsetModelOffset = src->QPOffsetModelOffset;
                    dst.m_QPOffsetModelScale = src->QPOffsetModelScale;
                    dst.m_CbQPoffset = src->CbQPOffset;
                    dst.m_CrQPoffset = src->CrQPOffset;
                    dst.m_numRefPicsActive = src->NumRefPicsActive;
                    dst.m_numRefPics = src->NumRefPics;
                    if (src->ReferencePics != nullptr) {
                        for (auto j = 0; j < src->NumRefPics && j < MAX_NUM_REF_PICS; j++) {
                            dst.m_referencePics[j] = src->ReferencePics[j];
                        }
                    }
                    if (src->UsedByCurrPic != nullptr) {
                        for (auto j = 0; j < src->NumRefPics && j < MAX_NUM_REF_PICS; j++) {
                            dst.m_usedByCurrPic[j] = src->UsedByCurrPic[j];
                        }
                    }
                    dst.m_betaOffsetDiv2 = src->BetaOffsetDiv2;
                    dst.m_tcOffsetDiv2 = src->TcOffsetDiv2;
                    dst.m_interRPSPrediction = src->InterRPSPrediction;
                    dst.m_deltaRPS = src->DeltaRPS;
                    dst.m_numRefIdc = src->NumRefIdc;
                    if (src->RefIdc != nullptr) {
                        for (auto j = 0; j < src->NumRefIdc && j < MAX_NUM_REF_PICS + 1; j++) {
                            dst.m_refIdc[j] = src->RefIdc[j];
                        }
                    }
                }

                // Create extra RPS entries for startup frames where some cross-GOP
                // references point to frames before the sequence start (absolute POC < 0).
                // This mirrors TAppEncCfg's verification logic in the HM reference encoder app.
                // selectReferencePictureSet() will pick these extra entries based on POCIndex.
                if (gopSize > 1) {
                    for (auto g = 0; g < GOPEntries->Length && g < MAX_GOP; g++) {
                        auto src = GOPEntries[g];
                        if (src == nullptr || src->ReferencePics == nullptr) {
                            continue;
                        }
                        auto curPOC = src->POC;
                        auto hasNegativePOC = false;
                        for (auto j = 0; j < src->NumRefPics && j < src->ReferencePics->Length; j++) {
                            if (curPOC + src->ReferencePics[j] < 0) {
                                hasNegativePOC = true;
                                break;
                            }
                        }
                        if (hasNegativePOC && gopSize + extraRPSCount < MAX_GOP) {
                            auto& extra = gopList[gopSize + extraRPSCount];
                            extra = gopList[g];
                            auto newRefCount = 0;
                            for (auto j = 0; j < src->NumRefPics && j < src->ReferencePics->Length; j++) {
                                if (curPOC + src->ReferencePics[j] >= 0) {
                                    extra.m_referencePics[newRefCount] = src->ReferencePics[j];
                                    extra.m_usedByCurrPic[newRefCount] = (src->UsedByCurrPic != nullptr && j < src->UsedByCurrPic->Length) ? src->UsedByCurrPic[j] : 1;
                                    newRefCount++;
                                }
                            }
                            extra.m_numRefPics = newRefCount;
                            if (extra.m_numRefPicsActive > newRefCount) {
                                extra.m_numRefPicsActive = newRefCount > 0 ? newRefCount : 1;
                            }
                            extra.m_interRPSPrediction = 0;
                            extraRPSCount++;
                        }
                    }
                }
            }
            encoder->setGopList(gopList.data());
        }
        encoder->setExtraRPSs(extraRPSCount);

        for (auto i = 0; i < MAX_TLAYER; i++) {
            if (gopSize == 1) {
                encoder->setNumReorderPics(0, i);
                encoder->setMaxDecPicBuffering(1, i);
            } else {
                encoder->setNumReorderPics(gopSize - 1, i);
                encoder->setMaxDecPicBuffering(maxDecPicBuf, i);
            }
        }

        // Lambda modifiers
        for (UInt i = 0; i < MAX_TLAYER; i++) {
            encoder->setLambdaModifier(i, static_cast<double>(1.0));
        }
        {
            std::vector<double> intraLambdaModifier;
            encoder->setIntraLambdaModifier(intraLambdaModifier);
        }
        encoder->setIntraQpFactor(static_cast<double>(-1.0));

        encoder->setQP(QP);
        encoder->setIntraQPOffset(0);
        encoder->setLambdaFromQPEnable(false);

        encoder->setAccessUnitDelimiter(false);
        encoder->setMaxTempLayer(maxTLayers);
        encoder->setUseAMP(true);

        // ---- Loop/Deblock Filter ----
        encoder->setLoopFilterDisable(LoopFilterDisable);
        encoder->setLoopFilterOffsetInPPS(false);
        encoder->setLoopFilterBetaOffset(0);
        encoder->setLoopFilterTcOffset(0);
        encoder->setDeblockingFilterMetric(0);

        // ---- Motion Search ----
        encoder->setDisableIntraPUsInInterSlices(false);
        encoder->setMotionEstimationSearchMethod(static_cast<MESearchMethod>(MotionEstimationSearchMethod));
        encoder->setSearchRange(SearchRange);
        encoder->setBipredSearchRange(BipredSearchRange);
        encoder->setClipForBiPredMeEnabled(false);
        encoder->setFastMEAssumingSmootherMVEnabled(true);
        encoder->setMinSearchWindow(8);
        encoder->setRestrictMESampling(false);

        // ---- Quality Control ----
        encoder->setMaxDeltaQP(0);
        encoder->setMaxCuDQPDepth(0);
        encoder->setDiffCuChromaQpOffsetDepth(0);
        encoder->setChromaCbQpOffset(0);
        encoder->setChromaCrQpOffset(0);
        {
            WCGChromaQPControl wcgCtrl = {};
            encoder->setWCGChromaQpControl(wcgCtrl);
        }
        {
            Int sliceChromaQpOffset[2] = { 0, 0 };
            encoder->setSliceChromaOffsetQpIntraOrPeriodic(0, sliceChromaQpOffset);
        }
        encoder->setChromaFormatIdc(static_cast<::ChromaFormat>(ChromaFormatIdc));

#if ADAPTIVE_QP_SELECTION
        encoder->setUseAdaptQpSelect(false);
#endif

#if JVET_V0078
        encoder->setSmoothQPReductionEnable(false);
        encoder->setSmoothQPReductionThreshold(static_cast<double>(0.0));
        encoder->setSmoothQPReductionModelScale(static_cast<double>(0.0));
        encoder->setSmoothQPReductionModelOffset(static_cast<double>(0.0));
        encoder->setSmoothQPReductionLimit(0);
        encoder->setSmoothQPReductionPeriodicity(0);
#endif

        encoder->setUseAdaptiveQP(false);
        encoder->setQPAdaptationRange(6);
        encoder->setExtendedPrecisionProcessingFlag(ExtendedPrecision);
        encoder->setHighPrecisionOffsetsEnabledFlag(HighPrecisionOffsets);

        encoder->setWeightedPredictionMethod(WP_PER_PICTURE_WITH_SIMPLE_DC_PER_COMPONENT);

        // ---- Tool List ----
        {
            LumaLevelToDeltaQPMapping lumaQpMapping = {};
            encoder->setLumaLevelToDeltaQPControls(lumaQpMapping);
        }
        encoder->setDeltaQpRD(0);
        encoder->setFastDeltaQp(false);
        encoder->setUseASR(false);
        encoder->setUseHADME(true);
        encoder->setdQPs(nullptr);
        encoder->setUseRDOQ(true);
        encoder->setUseRDOQTS(true);
        encoder->setUseSelectiveRDOQ(false);
        encoder->setRDpenalty(0);
        encoder->setMaxCUWidth(MaxCUWidth);
        encoder->setMaxCUHeight(MaxCUHeight);
        {
            // Derive MaxTotalCUDepth and Log2DiffMaxMinCodingBlockSize from user-configured
            // partition depth, mirroring TAppEncCfg.cpp lines 2245-2252.
            auto quadtreeTULog2MinSize = 2;
            UInt uiAddCUDepth = 0;
            while ((MaxCUWidth >> MaxTotalCUDepth) > (1 << (quadtreeTULog2MinSize + uiAddCUDepth))) {
                uiAddCUDepth++;
            }
            auto chromaFormat = static_cast<::ChromaFormat>(ChromaFormatIdc);
            auto chromaOffset = (chromaFormat == CHROMA_422 && quadtreeTULog2MinSize > 2) ? 1u : 0u;
            encoder->setMaxTotalCUDepth(MaxTotalCUDepth + uiAddCUDepth + chromaOffset);
            encoder->setLog2DiffMaxMinCodingBlockSize(MaxTotalCUDepth - 1);
        }
        encoder->setQuadtreeTULog2MaxSize(5);
        encoder->setQuadtreeTULog2MinSize(2);
        encoder->setQuadtreeTUMaxDepthInter(3);
        encoder->setQuadtreeTUMaxDepthIntra(3);
        encoder->setFastInterSearchMode(FASTINTERSEARCH_DISABLED);
        encoder->setUseEarlyCU(false);
        encoder->setUseFastDecisionForMerge(true);
        encoder->setUseCbfFastMode(false);
        encoder->setUseEarlySkipDetection(false);
        encoder->setCrossComponentPredictionEnabledFlag(false);
        encoder->setUseReconBasedCrossCPredictionEstimate(false);

        // SAO offset scale for high bit depth
        if (internalBitDepth > 10) {
            encoder->setLog2SaoOffsetScale(CHANNEL_TYPE_LUMA, static_cast<UInt>(internalBitDepth - 10));
            encoder->setLog2SaoOffsetScale(CHANNEL_TYPE_CHROMA, static_cast<UInt>(internalBitDepth - 10));
        } else {
            encoder->setLog2SaoOffsetScale(CHANNEL_TYPE_LUMA, 0);
            encoder->setLog2SaoOffsetScale(CHANNEL_TYPE_CHROMA, 0);
        }

        encoder->setUseTransformSkip(false);
        encoder->setUseTransformSkipFast(false);
        encoder->setTransformSkipRotationEnabledFlag(false);
        encoder->setTransformSkipContextEnabledFlag(false);
        encoder->setPersistentRiceAdaptationEnabledFlag(true);
        encoder->setCabacBypassAlignmentEnabledFlag(false);
        encoder->setLog2MaxTransformSkipBlockSize(2);
        for (UInt signallingModeIndex = 0; signallingModeIndex < NUMBER_OF_RDPCM_SIGNALLING_MODES; signallingModeIndex++) {
            encoder->setRdpcmEnabledFlag(RDPCMSignallingMode(signallingModeIndex), true);
        }
        encoder->setUseConstrainedIntraPred(false);
        encoder->setFastUDIUseMPMEnabled(true);
        encoder->setFastMEForGenBLowDelayEnabled(true);
        encoder->setUseBLambdaForNonKeyLowDelayPictures(false);
        encoder->setPCMLog2MinSize(3);
        encoder->setUsePCM(false);

        // ---- Bit Depth ----
        encoder->setBitDepth(CHANNEL_TYPE_LUMA, internalBitDepth);
        encoder->setBitDepth(CHANNEL_TYPE_CHROMA, internalBitDepth);
        encoder->setPCMBitDepth(CHANNEL_TYPE_LUMA, InputBitDepth);
        encoder->setPCMBitDepth(CHANNEL_TYPE_CHROMA, InputBitDepth);
        encoder->setBitDepthInput(CHANNEL_TYPE_LUMA, InputBitDepth);
        encoder->setBitDepthInput(CHANNEL_TYPE_CHROMA, InputBitDepth);

        encoder->setPCMLog2MaxSize(5);
        encoder->setMaxNumMergeCand(5);

        // ---- Weighted Prediction ----
        encoder->setUseWP(gopSize > 1);
        encoder->setWPBiPred(gopSize > 1);

        // ---- Parallel Merge ----
        encoder->setLog2ParallelMergeLevelMinus2(0);

        // ---- Slices ----
        encoder->setSliceMode(NO_SLICES);
        encoder->setSliceArgument(0);
        encoder->setSliceSegmentMode(NO_SLICES);
        encoder->setSliceSegmentArgument(0);
        encoder->setLFCrossSliceBoundaryFlag(true);

        // ---- SAO ----
        encoder->setUseSAO(UseSAO);
        encoder->setTestSAODisableAtPictureLevel(false);
        encoder->setSaoEncodingRate(0.75);
        encoder->setSaoEncodingRateChroma(0.5);
        encoder->setMaxNumOffsetsPerPic(2048);
        encoder->setSaoCtuBoundary(false);
        encoder->setResetEncoderStateAfterIRAP(false);
        encoder->setPCMInputBitDepthFlag(true);
        encoder->setPCMFilterDisableFlag(false);

        // ---- Range Extension ----
        encoder->setIntraSmoothingDisabledFlag(false);

        // ---- SEI ----
        encoder->setDecodedPictureHashSEIType(static_cast<::HashType>(DecodedPictureHashSEIType));
        encoder->setRecoveryPointSEIEnabled(false);
        encoder->setBufferingPeriodSEIEnabled(false);
        encoder->setPictureTimingSEIEnabled(false);

        // Tone Mapping (disabled)
        encoder->setToneMappingInfoSEIEnabled(false);
        encoder->setTMISEIToneMapId(0);
        encoder->setTMISEIToneMapCancelFlag(false);
        encoder->setTMISEIToneMapPersistenceFlag(true);
        encoder->setTMISEICodedDataBitDepth(8);
        encoder->setTMISEITargetBitDepth(8);
        encoder->setTMISEIModelID(0);
        encoder->setTMISEIMinValue(0);
        encoder->setTMISEIMaxValue(1023);
        encoder->setTMISEISigmoidMidpoint(512);
        encoder->setTMISEISigmoidWidth(960);
        {
            Int* nullIntPtr = nullptr;
            encoder->setTMISEIStartOfCodedInterva(nullIntPtr);
        }
        encoder->setTMISEINumPivots(0);
        {
            Int* nullIntPtr = nullptr;
            encoder->setTMISEICodedPivotValue(nullIntPtr);
            encoder->setTMISEITargetPivotValue(nullIntPtr);
        }
        encoder->setTMISEICameraIsoSpeedIdc(0);
        encoder->setTMISEICameraIsoSpeedValue(400);
        encoder->setTMISEIExposureIndexIdc(0);
        encoder->setTMISEIExposureIndexValue(400);
        encoder->setTMISEIExposureCompensationValueSignFlag(false);
        encoder->setTMISEIExposureCompensationValueNumerator(0);
        encoder->setTMISEIExposureCompensationValueDenomIdc(2);
        encoder->setTMISEIRefScreenLuminanceWhite(350);
        encoder->setTMISEIExtendedRangeWhiteLevel(800);
        encoder->setTMISEINominalBlackLevelLumaCodeValue(16);
        encoder->setTMISEINominalWhiteLevelLumaCodeValue(235);
        encoder->setTMISEIExtendedWhiteLevelLumaCodeValue(300);

        // Chroma Resampling Filter Hint (disabled)
        encoder->setChromaResamplingFilterHintEnabled(false);
        encoder->setChromaResamplingHorFilterIdc(0);
        encoder->setChromaResamplingVerFilterIdc(0);

        // Frame Packing (disabled)
        encoder->setFramePackingArrangementSEIEnabled(false);
        encoder->setFramePackingArrangementSEIType(0);
        encoder->setFramePackingArrangementSEIId(0);
        encoder->setFramePackingArrangementSEIQuincunx(0);
        encoder->setFramePackingArrangementSEIInterpretation(0);

        // Segmented Rect Frame Packing (disabled)
        encoder->setSegmentedRectFramePackingArrangementSEIEnabled(false);
        encoder->setSegmentedRectFramePackingArrangementSEICancel(0);
        encoder->setSegmentedRectFramePackingArrangementSEIType(0);
        encoder->setSegmentedRectFramePackingArrangementSEIPersistence(0);

        // Display Orientation (disabled)
        encoder->setDisplayOrientationSEIAngle(0);

        // Temporal Level 0 Index (disabled)
        encoder->setTemporalLevel0IndexSEIEnabled(false);

        // Gradual Decoding Refresh Info (disabled)
        encoder->setGradualDecodingRefreshInfoEnabled(false);

        // No Display (disabled)
        encoder->setNoDisplaySEITLayer(0);

        // Decoding Unit Info (disabled)
        encoder->setDecodingUnitInfoSEIEnabled(false);

        // SOP Description (disabled)
        encoder->setSOPDescriptionSEIEnabled(false);

        // Scalable Nesting (disabled)
        encoder->setScalableNestingSEIEnabled(false);

        // Phase Indication (disabled)
        encoder->setPhaseIndicationSEIEnabledFullResolution(false);
        encoder->setHorPhaseNumFullResolution(0);
        encoder->setHorPhaseDenMinus1FullResolution(0);
        encoder->setVerPhaseNumFullResolution(0);
        encoder->setVerPhaseDenMinus1FullResolution(0);

        // Modality Information (disabled)
        encoder->setMiSEIEnabled(false);
        encoder->setMiCancelFlag(true);
        encoder->setMiPersistenceFlag(false);
        encoder->setMiModalityType(0);
        encoder->setMiSpectrumRangePresentFlag(false);
        encoder->setMiMinWavelengthMantissa(0);
        encoder->setMiMinWavelengthExponentPlus15(0);
        encoder->setMiMaxWavelengthMantissa(0);
        encoder->setMiMaxWavelengthExponentPlus15(0);

        // TMCTS (disabled)
        encoder->setTMCTSSEIEnabled(false);
        encoder->setTMCTSSEITileConstraint(false);
        encoder->setTMCTSExtractionSEIEnabled(false);

        // Time Code (disabled)
        encoder->setTimeCodeSEIEnabled(false);
        encoder->setNumberOfTimeSets(0);

        // Knee Function (disabled)
        encoder->setKneeSEIEnabled(false);
        {
            TEncCfg::TEncSEIKneeFunctionInformation emptyKnee = {};
            encoder->setKneeFunctionInformationSEI(emptyKnee);
        }

        // Colour Content Volume (disabled)
        encoder->setCcvSEIEnabled(false);
        encoder->setCcvSEICancelFlag(true);
        encoder->setCcvSEIPersistenceFlag(false);
        encoder->setCcvSEIPrimariesPresentFlag(false);
        encoder->setCcvSEIMinLuminanceValuePresentFlag(false);
        encoder->setCcvSEIMaxLuminanceValuePresentFlag(false);
        encoder->setCcvSEIAvgLuminanceValuePresentFlag(false);
        for (Int i = 0; i < MAX_NUM_COMPONENT; i++) {
            encoder->setCcvSEIPrimariesX(0, i);
            encoder->setCcvSEIPrimariesY(0, i);
        }
        encoder->setCcvSEIMinLuminanceValue(0);
        encoder->setCcvSEIMaxLuminanceValue(0);
        encoder->setCcvSEIAvgLuminanceValue(0);

        // ERP (disabled)
        encoder->setErpSEIEnabled(false);
        encoder->setErpSEICancelFlag(true);
        encoder->setErpSEIPersistenceFlag(false);
        encoder->setErpSEIGuardBandFlag(false);
        encoder->setErpSEIGuardBandType(0);
        encoder->setErpSEILeftGuardBandWidth(0);
        encoder->setErpSEIRightGuardBandWidth(0);

        // Sphere Rotation (disabled)
        encoder->setSphereRotationSEIEnabled(false);
        encoder->setSphereRotationSEICancelFlag(true);
        encoder->setSphereRotationSEIPersistenceFlag(false);
        encoder->setSphereRotationSEIYaw(0);
        encoder->setSphereRotationSEIPitch(0);
        encoder->setSphereRotationSEIRoll(0);

        // Omni Viewport (disabled)
        encoder->setOmniViewportSEIEnabled(false);
        encoder->setOmniViewportSEIId(0);
        encoder->setOmniViewportSEICancelFlag(true);
        encoder->setOmniViewportSEIPersistenceFlag(false);
        encoder->setOmniViewportSEICntMinus1(0);
        {
            std::vector<Int> emptyInts(1, 0);
            std::vector<UInt> emptyUInts(1, 0);
            encoder->setOmniViewportSEIAzimuthCentre(emptyInts);
            encoder->setOmniViewportSEIElevationCentre(emptyInts);
            encoder->setOmniViewportSEITiltCentre(emptyInts);
            encoder->setOmniViewportSEIHorRange(emptyUInts);
            encoder->setOmniViewportSEIVerRange(emptyUInts);
        }

        encoder->setGopBasedTemporalFilterEnabled(false);
#if JVET_Y0077_BIM
        encoder->setBIM(false);
#endif

        // CMP (disabled)
        encoder->setCmpSEIEnabled(false);
        encoder->setCmpSEICmpCancelFlag(true);
        encoder->setCmpSEICmpPersistenceFlag(false);

        // RWP (disabled)
        encoder->setRwpSEIEnabled(false);
        encoder->setRwpSEIRwpCancelFlag(true);
        encoder->setRwpSEIRwpPersistenceFlag(false);
        encoder->setRwpSEIConstituentPictureMatchingFlag(false);
        encoder->setRwpSEINumPackedRegions(0);
        encoder->setRwpSEIProjPictureWidth(0);
        encoder->setRwpSEIProjPictureHeight(0);
        encoder->setRwpSEIPackedPictureWidth(0);
        encoder->setRwpSEIPackedPictureHeight(0);
        {
            std::vector<UChar> emptyUChars;
            std::vector<UInt> emptyUInts;
            std::vector<UShort> emptyUShorts;
            std::vector<Bool> emptyBools;
            encoder->setRwpSEIRwpTransformType(emptyUChars);
            encoder->setRwpSEIRwpGuardBandFlag(emptyBools);
            encoder->setRwpSEIProjRegionWidth(emptyUInts);
            encoder->setRwpSEIProjRegionHeight(emptyUInts);
            encoder->setRwpSEIRwpSEIProjRegionTop(emptyUInts);
            encoder->setRwpSEIProjRegionLeft(emptyUInts);
            encoder->setRwpSEIPackedRegionWidth(emptyUShorts);
            encoder->setRwpSEIPackedRegionHeight(emptyUShorts);
            encoder->setRwpSEIPackedRegionTop(emptyUShorts);
            encoder->setRwpSEIPackedRegionLeft(emptyUShorts);
            encoder->setRwpSEIRwpLeftGuardBandWidth(emptyUChars);
            encoder->setRwpSEIRwpRightGuardBandWidth(emptyUChars);
            encoder->setRwpSEIRwpTopGuardBandHeight(emptyUChars);
            encoder->setRwpSEIRwpBottomGuardBandHeight(emptyUChars);
            encoder->setRwpSEIRwpGuardBandNotUsedForPredFlag(emptyBools);
            encoder->setRwpSEIRwpGuardBandType(emptyUChars);
        }

        // Shutter Interval (disabled)
        encoder->setShutterFilterFlag(false);
        encoder->setSiiSEIEnabled(false);
        encoder->setSiiSEINumUnitsInShutterInterval(0);
        encoder->setSiiSEITimeScale(0);
        {
            std::vector<UInt> emptyUInts;
            encoder->setSiiSEISubLayerNumUnitsInSI(emptyUInts);
        }

        // Film Grain Characteristics (disabled)
        encoder->setFilmGrainCharactersticsSEIEnabled(false);
        encoder->setFilmGrainCharactersticsSEICancelFlag(true);
        encoder->setFilmGrainCharactersticsSEIPersistenceFlag(false);
        encoder->setFilmGrainCharactersticsSEIModelID(0);
        encoder->setFilmGrainCharactersticsSEISepColourDescPresent(false);
        encoder->setFilmGrainCharactersticsSEIBlendingModeID(0);
        encoder->setFilmGrainCharactersticsSEILog2ScaleFactor(0);
#if JVET_X0048_X0103_FILM_GRAIN
        encoder->setFilmGrainAnalysisEnabled(false);
#endif
        for (Int i = 0; i < MAX_NUM_COMPONENT; i++) {
            encoder->setFGCSEICompModelPresent(false, i);
        }

        // Content Light Level (disabled)
        encoder->setCLLSEIEnabled(false);
        encoder->setCLLSEIMaxContentLightLevel(0);
        encoder->setCLLSEIMaxPicAvgLightLevel(0);

        // Ambient Viewing Environment (disabled)
        encoder->setAmbientViewingEnvironmentSEIEnabled(false);
        encoder->setAmbientViewingEnvironmentSEIIlluminance(0);
        encoder->setAmbientViewingEnvironmentSEIAmbientLightX(0);
        encoder->setAmbientViewingEnvironmentSEIAmbientLightY(0);

        // Fisheye Video Info (disabled)
        encoder->setFviSEIDisabled();

        // Colour Remap (disabled)
        {
            std::string empty;
            encoder->setColourRemapInfoSEIFileRoot(empty);
        }

        // Mastering Display (disabled)
        {
            TComSEIMasteringDisplay emptyMD = {};
            encoder->setMasteringDisplaySEI(emptyMD);
        }

        // Alternative Transfer Characteristics (disabled)
        encoder->setSEIAlternativeTransferCharacteristicsSEIEnable(false);
        encoder->setSEIPreferredTransferCharacteristics(0);

        // Green Metadata (disabled)
        encoder->setSEIGreenMetadataInfoSEIEnable(false);
        encoder->setSEIGreenMetadataType(0);
        encoder->setSEIXSDMetricType(0);

        // Regional Nesting (disabled)
        {
            std::string empty;
            encoder->setRegionalNestingSEIFileRoot(empty);
        }

        // Annotated Region (disabled)
        {
            std::string empty;
            encoder->setAnnotatedRegionSEIFileRoot(empty);
        }

        // ---- Tiles (single tile) ----
        encoder->setTileUniformSpacingFlag(true);
        encoder->setNumColumnsMinus1(0);
        encoder->setNumRowsMinus1(0);
        encoder->xCheckGSParameters();
        encoder->setLFCrossTileBoundaryFlag(true);

        // ---- Wavefront (disabled) ----
        encoder->setEntropyCodingSyncEnabledFlag(false);

        encoder->setTMVPModeId(1);
        encoder->setUseScalingListId(SCALING_LIST_OFF);
        {
            std::string empty;
            encoder->setScalingListFileName(empty);
        }
        encoder->setSignDataHidingEnabledFlag(true);

        // ---- Rate Control ----
        encoder->setUseRateCtrl(UseRateCtrl);
        encoder->setTargetBitrate(TargetBitrate);
        encoder->setKeepHierBit(0);
        encoder->setLCULevelRC(true);
        encoder->setUseLCUSeparateModel(true);
        encoder->setInitialQP(0);
        encoder->setForceIntraQP(false);
        encoder->setCpbSaturationEnabled(false);
        encoder->setCpbSize(0);
        encoder->setInitialCpbFullness(static_cast<double>(0.9));

        // ---- Lossless ----
        encoder->setTransquantBypassEnabledFlag(Lossless);
        encoder->setCUTransquantBypassFlagForceValue(Lossless);
        encoder->setCostMode(Lossless ? COST_LOSSLESS_CODING : COST_STANDARD_LOSSY);
        encoder->setUseRecalculateQPAccordingToLambda(false);
        encoder->setUseStrongIntraSmoothing(true);

        // ---- Active Parameter Sets (disabled) ----
        encoder->setActiveParameterSetsSEIEnabled(false);

        // ---- VUI (disabled) ----
        encoder->setVuiParametersPresentFlag(false);
        encoder->setAspectRatioInfoPresentFlag(false);
        encoder->setAspectRatioIdc(0);
        encoder->setSarWidth(0);
        encoder->setSarHeight(0);
        encoder->setOverscanInfoPresentFlag(false);
        encoder->setOverscanAppropriateFlag(false);
        encoder->setVideoSignalTypePresentFlag(false);
        encoder->setVideoFormat(5);
        encoder->setVideoFullRangeFlag(false);
        encoder->setColourDescriptionPresentFlag(false);
        encoder->setColourPrimaries(2);
        encoder->setTransferCharacteristics(2);
        encoder->setMatrixCoefficients(2);
        encoder->setChromaLocInfoPresentFlag(false);
        encoder->setChromaSampleLocTypeTopField(0);
        encoder->setChromaSampleLocTypeBottomField(0);
        encoder->setNeutralChromaIndicationFlag(false);
        encoder->setDefaultDisplayWindow(0, 0, 0, 0);
        encoder->setFrameFieldInfoPresentFlag(false);
        encoder->setPocProportionalToTimingFlag(false);
        encoder->setNumTicksPocDiffOneMinus1(0);
        encoder->setBitstreamRestrictionFlag(false);
        encoder->setTilesFixedStructureFlag(false);
        encoder->setMotionVectorsOverPicBoundariesFlag(false);
        encoder->setMinSpatialSegmentationIdc(0);
        encoder->setMaxBytesPerPicDenom(2);
        encoder->setMaxBitsPerMinCuDenom(1);

        encoder->setLog2MaxMvLengthHorizontal(15);
        encoder->setLog2MaxMvLengthVertical(15);
        encoder->setEfficientFieldIRAPEnabled(false); // interlaced/field coding only; must be false for progressive content
        encoder->setHarmonizeGopFirstFieldCoupleEnabled(false); // interlaced/field coding only; must be false for progressive content

        // ---- Summary Output (disabled) ----
        {
            std::string empty;
            encoder->setSummaryOutFilename(empty);
            encoder->setSummaryPicFilenameBase(empty);
        }
        encoder->setSummaryVerboseness(0);

        // ---- SEI Manifest / Prefix Indication (disabled) ----
        encoder->setSEIManifestSEIEnabled(false);
        encoder->setSEIPrefixIndicationSEIEnabled(false);

        // ---- Print flags (all disabled for library usage) ----
        encoder->setPrintMSEBasedSequencePSNR(false);
        encoder->setPrintHexPsnr(false);
        encoder->setPrintFrameMSE(false);
        encoder->setPrintSequenceMSE(false);
        encoder->setPrintMSSSIM(false);
        encoder->setXPSNREnableFlag(false);
        for (Int id = 0; id < MAX_NUM_COMPONENT; id++) {
            encoder->setXPSNRWeight(static_cast<double>(1.0), ComponentID(id));
        }
    }
}
