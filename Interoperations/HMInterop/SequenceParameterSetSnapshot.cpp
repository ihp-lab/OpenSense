#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "SequenceParameterSetSnapshot.h"

namespace HMInterop {
    SequenceParameterSetSnapshot::SequenceParameterSetSnapshot(const TComSPS* sps) {
        _spsId = sps->getSPSId();

        auto nativeBitDepths = sps->getBitDepths();
        _bitDepths.Luma = nativeBitDepths.recon[CHANNEL_TYPE_LUMA];
        _bitDepths.Chroma = nativeBitDepths.recon[CHANNEL_TYPE_CHROMA];

        _chromaFormatIdc = static_cast<ChromaFormat>(sps->getChromaFormatIdc());
        _picWidthInLumaSamples = sps->getPicWidthInLumaSamples();
        _picHeightInLumaSamples = sps->getPicHeightInLumaSamples();
        _profileTierLevel = gcnew HMInterop::ProfileTierLevelSnapshot(sps->getPTL());
        _maxCUWidth = sps->getMaxCUWidth();
        _maxCUHeight = sps->getMaxCUHeight();
        _maxTLayers = sps->getMaxTLayers();

        // VUI is optional
        auto vuiParams = sps->getVuiParameters();
        if (vuiParams != nullptr) {
            _videoUsabilityInfo = gcnew HMInterop::VideoUsabilityInfoSnapshot(vuiParams);
        }
    }

    SequenceParameterSetSnapshot::SequenceParameterSetSnapshot(
        HMInterop::BitDepths bitDepths, ChromaFormat chromaFormatIdc, int width, int height)
        : _spsId(0)
        , _bitDepths(bitDepths)
        , _chromaFormatIdc(chromaFormatIdc)
        , _picWidthInLumaSamples(width)
        , _picHeightInLumaSamples(height)
        , _profileTierLevel(nullptr)
        , _videoUsabilityInfo(nullptr)
        , _maxCUWidth(0)
        , _maxCUHeight(0)
        , _maxTLayers(0) {
    }
}
