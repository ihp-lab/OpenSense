#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComList.h"
#include "TLibCommon/TComPicYuv.h"
#include "TLibCommon/TComPic.h"
#include "TLibDecoder/TDecTop.h"
#include "TLibDecoder/NALread.h"
#include "HMNativeHelpers.h"
#include <vector>
#include <algorithm>

// Native decoded picture info
struct NativeDecodedPic {
    TComPicYuv* recPicYuv;  // NOT owned - just a reference for copying
    int poc;
};

// Collect output pictures from the decoder's picture list
// Returns number of pictures; fills outputPics array (caller must delete[])
static int NativeCollectOutputPictures(TDecTop* decoder, Int& pocLastDisplay, NativeDecodedPic** outPics) {
    auto picList = decoder->getRpcListPic();
    if (!picList) {
        *outPics = nullptr;
        return 0;
    }

    // Collect pictures that are marked for output and have been reconstructed
    std::vector<TComPic*> outputPicVec;
    for (auto it = picList->begin(); it != picList->end(); it++) {
        auto pic = *it;
        if (pic && pic->getOutputMark() && pic->getReconMark() && pic->getPOC() > pocLastDisplay) {
            outputPicVec.push_back(pic);
        }
    }

    if (outputPicVec.empty()) {
        *outPics = nullptr;
        return 0;
    }

    // Sort by POC for display order
    std::sort(
        outputPicVec.begin(), outputPicVec.end(),
        [](TComPic* a, TComPic* b) { return a->getPOC() < b->getPOC(); }
    );

    auto count = static_cast<int>(outputPicVec.size());
    auto pics = new NativeDecodedPic[count];

    for (auto i = 0; i < count; i++) {
        auto pic = outputPicVec[i];
        pics[i].recPicYuv = pic->getPicYuvRec();
        pics[i].poc = pic->getPOC();

        // Mark as output done
        pocLastDisplay = pic->getPOC();
        pic->setOutputMark(false);
    }

    *outPics = pics;
    return count;
}

// Flush the decoder (execute loop filters and collect remaining pictures)
static int NativeFlushDecoder(TDecTop* decoder, Int& pocLastDisplay, NativeDecodedPic** outPics) {
    auto poc = 0;
    TComList<TComPic*>* picList = nullptr;
    decoder->executeLoopFilters(poc, picList);
    return NativeCollectOutputPictures(decoder, pocLastDisplay, outPics);
}

// Feed a single NAL unit to the decoder, returns decoded pictures if any
static int NativeFeedNal(
    TDecTop* decoder,
    const uint8_t* nalData,
    int nalSize,
    Int& pocLastDisplay,
    NativeDecodedPic** outPics
) {
    *outPics = nullptr;

    // Construct NAL unit from raw data
    std::vector<uint8_t> nalUnit(nalData, nalData + nalSize);
    InputNALUnit nalu;
    nalu.getBitstream().getFifo() = nalUnit;
    read(nalu);

    // Feed to decoder
    auto skipFrame = 0;
    auto newPicture = decoder->decode(nalu, skipFrame, pocLastDisplay);

    if (newPicture) {
        auto poc = 0;
        TComList<TComPic*>* picList = nullptr;
        decoder->executeLoopFilters(poc, picList);

        return NativeCollectOutputPictures(decoder, pocLastDisplay, outPics);
    }

    return 0;
}

#pragma managed(pop)

#include "Decoder.h"

using namespace System;

namespace HMInterop {
    Decoder::Decoder()
        : _decoder(nullptr)
        , _disposed(false)
        , _pocLastDisplay(-MAX_INT) { // HM's CommonDef.h

        _decoder = new TDecTop();
        _decoder->create();
        _decoder->init();
        _decoder->setDecodedPictureHashSEIEnabled(0);
    }

    cli::array<PicYuv^>^ Decoder::FeedNal([NotNull] cli::array<Byte>^ nalData) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(nalData, "nalData");

        if (nalData->Length == 0) {
            return Array::Empty<PicYuv^>();
        }

        // Pin managed array and feed to native decoder
        pin_ptr<Byte> pinnedData = &nalData[0];
        auto nativeData = static_cast<const uint8_t*>(pinnedData);

        // Copy managed member to local for native ref parameter
        auto pocLastDisplay = _pocLastDisplay;

        NativeDecodedPic* pics = nullptr;
        auto count = NativeFeedNal(_decoder, nativeData, nalData->Length, pocLastDisplay, &pics);

        _pocLastDisplay = pocLastDisplay;

        auto result = ConvertDecodedPics(pics, count);
        if (pics) {
            delete[] pics;
        }
        return result;
    }

    cli::array<PicYuv^>^ Decoder::FlushAndCollect() {
        ThrowIfDisposed();

        // Copy managed member to local for native ref parameter
        auto pocLastDisplay = _pocLastDisplay;

        std::vector<NativeDecodedPic> allPics;

        while (true) {
            NativeDecodedPic* flushPics = nullptr;
            auto flushCount = NativeFlushDecoder(_decoder, pocLastDisplay, &flushPics);
            if (flushCount == 0) {
                if (flushPics) {
                    delete[] flushPics;
                }
                break;
            }
            for (auto i = 0; i < flushCount; i++) {
                allPics.push_back(flushPics[i]);
            }
            delete[] flushPics;
        }

        _pocLastDisplay = pocLastDisplay;

        return ConvertDecodedPics(allPics.data(), static_cast<int>(allPics.size()));
    }

    cli::array<PicYuv^>^ Decoder::ConvertDecodedPics(void* picsPtr, int count) {
        auto pics = static_cast<NativeDecodedPic*>(picsPtr);
        if (!pics || count == 0) {
            return Array::Empty<PicYuv^>();
        }

        auto results = gcnew cli::array<PicYuv^>(count);
        for (auto i = 0; i < count; i++) {
            auto src = pics[i].recPicYuv;
            auto chromaFormat = static_cast<HMInterop::ChromaFormat>(src->getChromaFormat());
            auto decodedPicture = gcnew PicYuv(chromaFormat, src->getWidth(COMPONENT_Y), src->getHeight(COMPONENT_Y));

            // Copy pixel data from decoded picture to our wrapper
            CopyPicYuvData(src, decodedPicture->InternalPicYuv);

            results[i] = decodedPicture;
        }

        return results;
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
