#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComList.h"
#include "TLibCommon/TComPicYuv.h"
#include "TLibCommon/AccessUnit.h"
#include "TLibEncoder/TEncTop.h"
#include "TLibEncoder/AnnexBwrite.h"
#include "HMNativeHelpers.h"
#include <sstream>
#include <map>
#include <vector>

// Native helper struct for encoded output
struct NativeEncodedUnit {
    std::string data;
    int poc;
    long long pts;
    bool ptsFound;
};

// Create a TComList<TComPicYuv*>
static void* CreateRecPicList() {
    return new TComList<TComPicYuv*>();
}


// Create a PTS map
static void* CreatePtsMap() {
    return new std::map<Int, long long>();
}

// Destroy a PTS map
static void DestroyPtsMap(void* ptr) {
    if (ptr) {
        delete static_cast<std::map<Int, long long>*>(ptr);
    }
}

// Record PTS in the map
static void RecordPts(void* ptsMapPtr, Int frameIndex, long long pts) {
    auto ptsMap = static_cast<std::map<Int, long long>*>(ptsMapPtr);
    (*ptsMap)[frameIndex] = pts;
}

// Push a reconstruction buffer into recPicList before each encode() call.
// TEncGOP::xGetBuffer iterates backwards from end(), so the list must
// already contain at least one entry per received picture.
// Mirrors TAppEncTop::xGetBuffer in the reference application.
static void PrepareRecBuffer(
    TComList<TComPicYuv*>* recPicList,
    Int gopSize,
    Int width,
    Int height,
    ChromaFormat chromaFormat,
    UInt maxCUWidth,
    UInt maxCUHeight,
    UInt maxCUDepth
) {
    TComPicYuv* recBuf;
    if (static_cast<Int>(recPicList->size()) >= gopSize) {
        recBuf = recPicList->popFront();
    } else {
        recBuf = new TComPicYuv;
        recBuf->create(width, height, chromaFormat, maxCUWidth, maxCUHeight, maxCUDepth, true);
    }
    recPicList->pushBack(recBuf);
}

// Destroy all reconstruction buffers in recPicList
static void DestroyRecBuffers(TComList<TComPicYuv*>* recPicList) {
    for (auto it = recPicList->begin(); it != recPicList->end(); it++) {
        (*it)->destroy();
        delete *it;
    }
    recPicList->clear();
}

// Perform native encode and return results
// Returns number of encoded units; fills outputUnits array (caller must delete[])
static int NativeEncode(
    TEncTop* encoder,
    bool flush,
    TComPicYuv* picOrg,
    void* recPicListPtr,
    void* ptsMapPtr,
    int inputFrameCountFallback,
    NativeEncodedUnit** outUnits
) {
    auto recPicList = static_cast<TComList<TComPicYuv*>*>(recPicListPtr);
    auto ptsMap = static_cast<std::map<Int, long long>*>(ptsMapPtr);
    std::list<AccessUnit> accessUnitsOut;
    auto numEncoded = 0;

    // In streaming mode, framesToBeEncoded is INT_MAX. Before flush, set it to
    // the actual frame count so compressGOP's guard condition correctly skips
    // GOP entries beyond the last received frame (prevents xGetBuffer crash).
    if (flush) {
        encoder->setFramesToBeEncoded(inputFrameCountFallback);
    }

    encoder->encode(
        flush,
        picOrg,
        picOrg,          // true original (same for us)
        nullptr,         // film grain filtered (JVET_X0048_X0103_FILM_GRAIN)
        IPCOLOURSPACE_UNCHANGED,
        IPCOLOURSPACE_UNCHANGED,
        *recPicList,
        accessUnitsOut,
        numEncoded
    );

    if (numEncoded == 0 || accessUnitsOut.empty()) {
        *outUnits = nullptr;
        return 0;
    }

    auto gopSize = encoder->getGOPSize();
    auto iPOCLast = inputFrameCountFallback - 1;
    // HM fires compressGOP after the very first frame (iPOCLast==0) as a
    // special single-frame batch.  This consumes 1 frame before the regular
    // GOP cycling begins, so the remaining count for flush is off-by-one
    // compared to a naive modulus.
    auto iNumPicRcvd = flush
        ? ((inputFrameCountFallback - 1) % gopSize)
        : gopSize;

    // Replicate compressGOP's iGOPid iteration to determine the POC
    // of each output AccessUnit, since recPicList pointer matching
    // does not work (HM's internal reconstruction buffers are separate
    // objects from the ones we provide via rcListPicYuvRecOut).
    std::vector<int> codingOrderPOCs;
    for (auto iGOPid = 0; iGOPid < gopSize; iGOPid++) {
        auto pocCurr = (iPOCLast == 0)
            ? 0
            : iPOCLast - iNumPicRcvd + encoder->getGOPEntry(iGOPid).m_POC;
        if (pocCurr >= encoder->getFramesToBeEncoded()) {
            continue;
        }
        codingOrderPOCs.push_back(pocCurr);
    }

    auto units = new NativeEncodedUnit[numEncoded];
    auto count = 0;
    auto auIt = accessUnitsOut.begin();

    for (auto i = 0; i < numEncoded && auIt != accessUnitsOut.end(); i++, ++auIt) {
        auto poc = (i < static_cast<int>(codingOrderPOCs.size()))
            ? codingOrderPOCs[i]
            : (inputFrameCountFallback - 1);

        auto framePts = 0LL;
        auto found = false;
        auto it = ptsMap->find(poc);
        if (it != ptsMap->end()) {
            framePts = it->second;
            found = true;
            ptsMap->erase(it);
        }

        std::ostringstream stream;
        writeAnnexB(stream, *auIt);
        auto str = stream.str();

        if (!str.empty()) {
            units[count].data = std::move(str);
            units[count].poc = poc;
            units[count].pts = framePts;
            units[count].ptsFound = found;
            count++;
        }
    }

    *outUnits = units;
    return count;
}

// Accessor for TEncCfg protected members
class TEncTopAccessor : public TEncTop {
public:
    static UInt GetMaxCUWidth(TEncTop* enc) {
        return static_cast<TEncTopAccessor*>(enc)->m_maxCUWidth;
    }
    static UInt GetMaxCUHeight(TEncTop* enc) {
        return static_cast<TEncTopAccessor*>(enc)->m_maxCUHeight;
    }
    static UInt GetMaxTotalCUDepth(TEncTop* enc) {
        return static_cast<TEncTopAccessor*>(enc)->m_maxTotalCUDepth;
    }
};

#pragma managed(pop)

#include "Encoder.h"
#include "HMContext.h"

using namespace System;
using namespace System::Buffers;
using namespace System::Collections::Generic;

namespace HMInterop {
    Encoder::Encoder([NotNull] EncoderConfig^ config)
        : _config(config)
        , _encoder(nullptr)
        , _ptsMap(nullptr)
        , _inputFrameCount(0)
        , _recPicList(nullptr)
        , _maxWidth(0)
        , _maxHeight(0)
        , _maxDepth(0)
        , _disposed(false) {

        ArgumentNullException::ThrowIfNull(config, "config");

        _encoder = new TEncTop();
        _ptsMap = CreatePtsMap();
        _recPicList = CreateRecPicList();

        // Apply configuration
        config->ApplyTo(_encoder);

        // Use config values for initial Acquire (approximate; corrected after create)
        _maxWidth = config->MaxCUWidth;
        _maxHeight = config->MaxCUHeight;
        _maxDepth = config->MaxTotalCUDepth;

        // Initialize encoder under HMContext lock
        HMContext::Acquire(_maxWidth, _maxHeight, _maxDepth);
        try {
            HMContext::PrepareForInitialization();
            _encoder->create();
            _encoder->init(false);
            HMContext::NotifyInitializationComplete();

            // Read actual CTU parameters from encoder (may differ from config after internal derivation)
            _maxWidth = TEncTopAccessor::GetMaxCUWidth(_encoder);
            _maxHeight = TEncTopAccessor::GetMaxCUHeight(_encoder);
            _maxDepth = TEncTopAccessor::GetMaxTotalCUDepth(_encoder);
        }
        finally {
            HMContext::Release();
        }
    }

    void Encoder::Encode(
        [NotNull] PictureYuv^ inputPicture, long long pts,
        [NotNull] System::Collections::Generic::IList<AccessUnitData^>^ output
    ) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(inputPicture, "inputPicture");
        ArgumentNullException::ThrowIfNull(output, "output");

        HMContext::Acquire(_maxWidth, _maxHeight, _maxDepth);
        try {
            // Create internal TComPicYuv for the encoder
            auto picOrg = new TComPicYuv();
            picOrg->createWithoutCUInfo(_encoder->getSourceWidth(), _encoder->getSourceHeight(), _encoder->getChromaFormatIdc(), false);

            // Copy pixel data from input picture to internal buffer
            CopyPicYuvData(inputPicture->InternalPicYuv, picOrg);

            // Record PTS mapping
            RecordPts(_ptsMap, _inputFrameCount, pts);
            _inputFrameCount++;

            // Push reconstruction buffer (required by TEncGOP::xGetBuffer)
            PrepareRecBuffer(
                static_cast<TComList<TComPicYuv*>*>(_recPicList),
                _config->GOPEntries->Length,
                _config->SourceWidth,
                _config->SourceHeight,
                static_cast<::ChromaFormat>(_config->ChromaFormatIdc),
                _maxWidth,
                _maxHeight,
                _maxDepth
            );

            // Encode
            NativeEncodedUnit* outUnits = nullptr;
            auto count = NativeEncode(
                _encoder,
                false,
                picOrg,
                _recPicList,
                _ptsMap,
                _inputFrameCount,
                &outUnits
            );

            // Collect output: copy native data to pooled managed buffers
            for (auto i = 0; i < count; i++) {
                if (!outUnits[i].ptsFound) {
                    throw gcnew InvalidOperationException(System::String::Format("Could not find PTS for POC {0}", outUnits[i].poc));
                }
                auto length = static_cast<int>(outUnits[i].data.size());
                auto owner = MemoryPool<Byte>::Shared->Rent(length);
                {
                    auto handle = owner->Memory.Pin();
                    memcpy(handle.Pointer, outUnits[i].data.data(), length);
                    delete safe_cast<IDisposable^>(handle);
                }
                output->Add(gcnew AccessUnitData(owner, length, outUnits[i].pts, outUnits[i].poc));
            }
            if (outUnits) {
                delete[] outUnits;
            }

            // Clean up the input buffer
            picOrg->destroy();
            delete picOrg;
        }
        finally {
            HMContext::Release();
        }
    }

    void Encoder::Flush([NotNull] System::Collections::Generic::IList<AccessUnitData^>^ output) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(output, "output");

        HMContext::Acquire(_maxWidth, _maxHeight, _maxDepth);
        try {
            NativeEncodedUnit* outUnits = nullptr;
            auto count = NativeEncode(
                _encoder,
                true,
                nullptr,
                _recPicList,
                _ptsMap,
                _inputFrameCount,
                &outUnits
            );

            for (auto i = 0; i < count; i++) {
                if (!outUnits[i].ptsFound) {
                    throw gcnew InvalidOperationException(System::String::Format("Could not find PTS for POC {0}", outUnits[i].poc));
                }
                auto length = static_cast<int>(outUnits[i].data.size());
                auto owner = MemoryPool<Byte>::Shared->Rent(length);
                {
                    auto handle = owner->Memory.Pin();
                    memcpy(handle.Pointer, outUnits[i].data.data(), length);
                    delete safe_cast<IDisposable^>(handle);
                }
                output->Add(gcnew AccessUnitData(owner, length, outUnits[i].pts, outUnits[i].poc));
            }
            if (outUnits) {
                delete[] outUnits;
            }
        }
        finally {
            HMContext::Release();
        }
    }

#pragma region IDisposable
    void Encoder::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Encoder::~Encoder() {
        if (_disposed) {
            return;
        }

        this->!Encoder();
        _disposed = true;
    }

    Encoder::!Encoder() {
        HMContext::Acquire(_maxWidth, _maxHeight, _maxDepth);
        try {
            if (_encoder) {
                _encoder->destroy();
                delete _encoder;
                _encoder = nullptr;
            }
            HMContext::NotifyDestructionComplete();
        }
        finally {
            HMContext::Release();
        }

        DestroyPtsMap(_ptsMap);
        _ptsMap = nullptr;

        if (_recPicList) {
            DestroyRecBuffers(static_cast<TComList<TComPicYuv*>*>(_recPicList));
            delete static_cast<TComList<TComPicYuv*>*>(_recPicList);
            _recPicList = nullptr;
        }
    }
#pragma endregion
}
