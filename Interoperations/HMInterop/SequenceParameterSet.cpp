#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "SequenceParameterSet.h"

namespace HMInterop {
    SequenceParameterSet::SequenceParameterSet(const TComSPS* source)
        : _sps(nullptr)
        , _sourcePtr(source)
        , _profileTierLevel(nullptr)
        , _videoUsabilityInfo(nullptr)
        , _disposed(false) {

        if (!source) {
            throw gcnew ArgumentNullException("source");
        }
        _sps = new TComSPS(*source);
    }

    SequenceParameterSet::SequenceParameterSet(
        HMInterop::BitDepths bitDepths, ChromaFormat chromaFormatIdc, int width, int height)
        : _sps(nullptr)
        , _sourcePtr(nullptr)
        , _profileTierLevel(nullptr)
        , _videoUsabilityInfo(nullptr)
        , _disposed(false) {

        _sps = new TComSPS();
        _sps->setBitDepth(CHANNEL_TYPE_LUMA, bitDepths.Luma);
        _sps->setBitDepth(CHANNEL_TYPE_CHROMA, bitDepths.Chroma);
        _sps->setChromaFormatIdc(static_cast<::ChromaFormat>(chromaFormatIdc));
        _sps->setPicWidthInLumaSamples(width);
        _sps->setPicHeightInLumaSamples(height);
    }

    int SequenceParameterSet::SpsId::get() {
        ThrowIfDisposed();
        return _sps->getSPSId();
    }

    HMInterop::BitDepths SequenceParameterSet::BitDepths::get() {
        ThrowIfDisposed();
        auto native = _sps->getBitDepths();
        HMInterop::BitDepths result;
        result.Luma = native.recon[CHANNEL_TYPE_LUMA];
        result.Chroma = native.recon[CHANNEL_TYPE_CHROMA];
        return result;
    }

    ChromaFormat SequenceParameterSet::ChromaFormatIdc::get() {
        ThrowIfDisposed();
        return static_cast<ChromaFormat>(_sps->getChromaFormatIdc());
    }

    int SequenceParameterSet::PicWidthInLumaSamples::get() {
        ThrowIfDisposed();
        return _sps->getPicWidthInLumaSamples();
    }

    int SequenceParameterSet::PicHeightInLumaSamples::get() {
        ThrowIfDisposed();
        return _sps->getPicHeightInLumaSamples();
    }

    HMInterop::ProfileTierLevel^ SequenceParameterSet::ProfileTierLevel::get() {
        ThrowIfDisposed();
        if (_profileTierLevel == nullptr) {
            auto ptl = _sps->getPTL();
            if (ptl != nullptr) {
                _profileTierLevel = gcnew HMInterop::ProfileTierLevel(this, ptl);
            }
        }
        return _profileTierLevel;
    }

    HMInterop::VideoUsabilityInfo^ SequenceParameterSet::VideoUsabilityInfo::get() {
        ThrowIfDisposed();
        if (_videoUsabilityInfo == nullptr) {
            auto vui = _sps->getVuiParameters();
            if (vui != nullptr) {
                _videoUsabilityInfo = gcnew HMInterop::VideoUsabilityInfo(this, vui);
            }
        }
        return _videoUsabilityInfo;
    }

    int SequenceParameterSet::MaxCUWidth::get() {
        ThrowIfDisposed();
        return _sps->getMaxCUWidth();
    }

    int SequenceParameterSet::MaxCUHeight::get() {
        ThrowIfDisposed();
        return _sps->getMaxCUHeight();
    }

    int SequenceParameterSet::MaxTLayers::get() {
        ThrowIfDisposed();
        return _sps->getMaxTLayers();
    }

    bool SequenceParameterSet::Equals(Object^ other) {
        auto o = dynamic_cast<SequenceParameterSet^>(other);
        if (o == nullptr) {
            return false;
        }
        return SpsId == o->SpsId
            && BitDepths.Luma == o->BitDepths.Luma
            && BitDepths.Chroma == o->BitDepths.Chroma
            && ChromaFormatIdc == o->ChromaFormatIdc
            && PicWidthInLumaSamples == o->PicWidthInLumaSamples
            && PicHeightInLumaSamples == o->PicHeightInLumaSamples
            && MaxCUWidth == o->MaxCUWidth
            && MaxCUHeight == o->MaxCUHeight
            && MaxTLayers == o->MaxTLayers
            && Object::Equals(ProfileTierLevel, o->ProfileTierLevel)
            && Object::Equals(VideoUsabilityInfo, o->VideoUsabilityInfo);
    }

    int SequenceParameterSet::GetHashCode() {
        return SpsId
             ^ (BitDepths.Luma << 4)
             ^ (BitDepths.Chroma << 12)
             ^ (static_cast<int>(ChromaFormatIdc) << 20)
             ^ PicWidthInLumaSamples
             ^ (PicHeightInLumaSamples << 16);
    }

    TComSPS* SequenceParameterSet::InternalSps::get() {
        ThrowIfDisposed();
        return _sps;
    }

    const void* SequenceParameterSet::SourcePtr::get() {
        return _sourcePtr;
    }

#pragma region IDisposable
    void SequenceParameterSet::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    SequenceParameterSet::~SequenceParameterSet() {
        if (_disposed) {
            return;
        }

        this->!SequenceParameterSet();
        _disposed = true;
    }

    SequenceParameterSet::!SequenceParameterSet() {
        if (_sps) {
            delete _sps;
            _sps = nullptr;
        }
    }
#pragma endregion
}
