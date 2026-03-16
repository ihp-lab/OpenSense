#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComList.h"
#include "TLibCommon/TComPic.h"
#include "TLibDecoder/TDecTop.h"
#include "TLibDecoder/NALread.h"
#include "HMNativeHelpers.h"
#include <vector>
#include <algorithm>

// Native decoded picture info
struct NativeDecodedPic {
    TComPic* comPic;
};

// Collect output pictures using DPB bumping logic (matches HM's xWriteOutput).
// Only bumps (outputs) the lowest-POC picture when the DPB exceeds the SPS
// reorder or buffering limits, ensuring pictures are output in POC order.
static void NativeCollectOutputPictures(TDecTop* decoder, Int& pocLastDisplay, std::vector<NativeDecodedPic>& output) {
    auto picList = decoder->getRpcListPic();
    if (!picList || picList->empty()) {
        return;
    }

    // Find active SPS from a valid (reconstructed) picture
    const TComSPS* activeSPS = nullptr;
    for (auto it = picList->begin(); it != picList->end(); it++) {
        if ((*it)->getReconMark()) {
            activeSPS = &((*it)->getPicSym()->getSPS());
            break;
        }
    }
    if (!activeSPS) {
        return;
    }

    auto maxTLayers = activeSPS->getMaxTLayers();
    auto numReorderPics = static_cast<int>(activeSPS->getNumReorderPics(maxTLayers - 1));
    auto maxDecPicBuffering = static_cast<int>(activeSPS->getMaxDecPicBuffering(maxTLayers - 1));

    // Bumping loop: output lowest-POC picture when DPB management requires it
    while (true) {
        auto numPicsNotYetDisplayed = 0;
        auto dpbFullness = 0;
        TComPic* lowestPocPic = nullptr;

        for (auto it = picList->begin(); it != picList->end(); it++) {
            auto pic = *it;
            if (!pic->getOutputMark() || pic->getPOC() <= pocLastDisplay) {
                if (pic->getSlice(0)->isReferenced()) {
                    dpbFullness++;
                }
            } else {
                numPicsNotYetDisplayed++;
                dpbFullness++;
                if (!lowestPocPic || pic->getPOC() < lowestPocPic->getPOC()) {
                    lowestPocPic = pic;
                }
            }
        }

        if (!lowestPocPic ||
            (numPicsNotYetDisplayed <= numReorderPics && dpbFullness <= maxDecPicBuffering)) {
            break;
        }

        // Bump: output this picture
        pocLastDisplay = lowestPocPic->getPOC();
        lowestPocPic->setOutputMark(false);
        if (!lowestPocPic->getSlice(0)->isReferenced()) {
            lowestPocPic->setReconMark(false);
            lowestPocPic->getPicYuvRec()->setBorderExtension(false);
        }
        output.push_back({ lowestPocPic });
    }
}

// Flush all remaining output pictures (no bumping condition, used at EOF and IDR boundaries).
static void NativeFlushOutputPictures(TDecTop* decoder, Int& pocLastDisplay, std::vector<NativeDecodedPic>& output) {
    auto picList = decoder->getRpcListPic();
    if (!picList || picList->empty()) {
        return;
    }

    std::vector<TComPic*> pics;
    for (auto it = picList->begin(); it != picList->end(); it++) {
        auto pic = *it;
        if (pic && pic->getOutputMark() && pic->getReconMark() && pic->getPOC() > pocLastDisplay) {
            pics.push_back(pic);
        }
    }

    std::sort(pics.begin(), pics.end(),
        [](TComPic* a, TComPic* b) { return a->getPOC() < b->getPOC(); });

    for (auto pic : pics) {
        pocLastDisplay = pic->getPOC();
        pic->setOutputMark(false);
        output.push_back({ pic });
    }
}

// Handle IRAP boundary: flush/clear DPB as needed before re-feeding IRAP NAL.
// Matches HM's TAppDecTop lines 328-341.
static void NativeHandleIrapBoundary(TDecTop* decoder, NalUnitType nalType, Int& pocLastDisplay, std::vector<NativeDecodedPic>& output) {
    // CRA + NoOutputPriorPics: discard old pictures without outputting
    if (decoder->getNoOutputPriorPicsFlag()) {
        decoder->checkNoOutputPriorPics(decoder->getRpcListPic());
        decoder->setNoOutputPriorPicsFlag(false);
    }

    // IDR/BLA: flush all remaining output pictures, then clear DPB for new sequence
    if (nalType == NAL_UNIT_CODED_SLICE_IDR_W_RADL ||
        nalType == NAL_UNIT_CODED_SLICE_IDR_N_LP ||
        nalType == NAL_UNIT_CODED_SLICE_BLA_N_LP ||
        nalType == NAL_UNIT_CODED_SLICE_BLA_W_RADL ||
        nalType == NAL_UNIT_CODED_SLICE_BLA_W_LP) {
        NativeFlushOutputPictures(decoder, pocLastDisplay, output);

        // Clear DPB marks  -- IDR/BLA starts a fresh sequence, old pictures are irrelevant.
        // HM's reference decoder destroys all TComPic objects here (xFlushOutput);
        // we clear marks instead, allowing the decoder to reuse the buffers.
        auto rpcList = decoder->getRpcListPic();
        for (auto it = rpcList->begin(); it != rpcList->end(); it++) {
            auto pic = *it;
            pic->setOutputMark(false);
            pic->setReconMark(false);
            pic->getPicYuvRec()->setBorderExtension(false);
        }

        // Reset pocLastDisplay  -- IDR resets POC to 0, so the old value
        // (from the previous sequence) would prevent new frames from being output.
        pocLastDisplay = -MAX_INT;
    }
}

// Re-feed a NAL unit that was peeked but not decoded by the first decode() call.
// Must be called AFTER snapshots are taken from the bumped pictures, because
// re-feed may reuse TComPic buffers that were released by bumping.
static void NativeReFeedNal(TDecTop* decoder, const uint8_t* nalData, int nalSize, Int& pocLastDisplay) {
    auto skipFrame = 0;
    InputNALUnit nalu;
    nalu.getBitstream().getFifo() = std::vector<uint8_t>(nalData, nalData + nalSize);
    read(nalu);
    decoder->decode(nalu, skipFrame, pocLastDisplay);
}

// Feed a single NAL unit to the decoder.
// When newPicture is detected, collects output pictures but does NOT re-feed yet.
// Sets needsReFeed to true if the caller must call NativeReFeedNal after taking snapshots.
static void NativeFeedNal(
    TDecTop* decoder,
    const uint8_t* nalData,
    int nalSize,
    Int& pocLastDisplay,
    std::vector<NativeDecodedPic>& output,
    bool& needsReFeed
) {
    needsReFeed = false;

    // Construct NAL unit from raw data
    std::vector<uint8_t> nalUnit(nalData, nalData + nalSize);
    InputNALUnit nalu;
    nalu.getBitstream().getFifo() = nalUnit;
    read(nalu);

    // Feed to decoder
    auto skipFrame = 0;
    auto newPicture = decoder->decode(nalu, skipFrame, pocLastDisplay);

    if (newPicture) {
        // newPicture means the PREVIOUS picture is complete, but the current
        // NAL was only peeked (slice header parsed) and NOT actually decoded.
        // The caller must re-feed it AFTER taking snapshots of the bumped pictures,
        // because re-feed may reuse their TComPic buffers.
        needsReFeed = true;

        auto poc = 0;
        TComList<TComPic*>* picList = nullptr;
        decoder->executeLoopFilters(poc, picList);

        NativeCollectOutputPictures(decoder, pocLastDisplay, output);
        NativeHandleIrapBoundary(decoder, nalu.m_nalUnitType, pocLastDisplay, output);
    }
}

// Flush the decoder (execute loop filters and collect all remaining pictures)
static void NativeFlushDecoder(TDecTop* decoder, Int& pocLastDisplay, std::vector<NativeDecodedPic>& output) {
    auto poc = 0;
    TComList<TComPic*>* picList = nullptr;
    decoder->executeLoopFilters(poc, picList);
    NativeFlushOutputPictures(decoder, pocLastDisplay, output);
}

#pragma managed(pop)

#include "Decoder.h"
#include "PictureYuvPool.h"

using namespace System;

namespace HMInterop {
    Decoder::Decoder()
        : _decoder(nullptr)
        , _disposed(false)
        , _pocLastDisplay(-MAX_INT)
        , _lastSps(nullptr) {

        _decoder = new TDecTop();
        _decoder->create();
        _decoder->init();
        _decoder->setDecodedPictureHashSEIEnabled(0);
    }

    void Decoder::FeedNal(
        ReadOnlyMemory<Byte> nalData,
        [NotNull] System::Collections::Generic::IList<Picture^>^ output
    ) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(output, "output");

        auto length = nalData.Length;
        if (length == 0) {
            return;
        }

        // Pin managed memory and feed to native decoder
        auto handle = nalData.Pin();
        auto nativeData = static_cast<const uint8_t*>(handle.Pointer);

        auto pocLastDisplay = _pocLastDisplay;

        std::vector<NativeDecodedPic> nativeOutput;
        auto needsReFeed = false;
        NativeFeedNal(_decoder, nativeData, length, pocLastDisplay, nativeOutput, needsReFeed);

        // Create snapshots BEFORE re-feed, because re-feed may reuse TComPic buffers
        AppendDecodedPics(nativeOutput, output);

        // Now re-feed the NAL that was peeked but not decoded
        if (needsReFeed) {
            NativeReFeedNal(_decoder, nativeData, length, pocLastDisplay);
        }

        delete safe_cast<IDisposable^>(handle);
        _pocLastDisplay = pocLastDisplay;
    }

    void Decoder::FlushAndCollect(
        [NotNull] System::Collections::Generic::IList<Picture^>^ output
    ) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(output, "output");

        auto pocLastDisplay = _pocLastDisplay;

        while (true) {
            std::vector<NativeDecodedPic> nativeOutput;
            NativeFlushDecoder(_decoder, pocLastDisplay, nativeOutput);
            if (nativeOutput.empty()) {
                break;
            }
            AppendDecodedPics(nativeOutput, output);
        }

        _pocLastDisplay = pocLastDisplay;
    }

    void Decoder::AppendDecodedPics(
        const std::vector<NativeDecodedPic>& pics,
        System::Collections::Generic::IList<Picture^>^ output
    ) {
        for (auto i = 0; i < static_cast<int>(pics.size()); i++) {
            auto comPic = pics[i].comPic;
            auto srcYuv = comPic->getPicYuvRec();
            auto chromaFormat = static_cast<ChromaFormat>(srcYuv->getChromaFormat());
            auto width = srcYuv->getWidth(COMPONENT_Y);
            auto height = srcYuv->getHeight(COMPONENT_Y);

            auto picYuv = PictureYuvPool::Rent(chromaFormat, width, height);
            CopyPicYuvData(srcYuv, picYuv->InternalPicYuv);

            auto poc = comPic->getPOC();
            auto spsPtr = comPic->getSlice(0)->getSPS();
            if (_lastSps == nullptr || spsPtr != _lastSps->SourcePtr) {
                _lastSps = gcnew SequenceParameterSet(spsPtr);
            }
            auto sps = _lastSps;
            output->Add(gcnew Picture(picYuv, poc, sps, PictureYuvOwnership::Pooled));
        }
    }

#pragma region IDisposable
    void Decoder::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Decoder::~Decoder() {
        if (_disposed) {
            return;
        }

        this->!Decoder();
        _disposed = true;
    }

    Decoder::!Decoder() {
        if (_decoder) {
            _decoder->destroy();
            delete _decoder;
            _decoder = nullptr;
        }
    }
#pragma endregion
}
