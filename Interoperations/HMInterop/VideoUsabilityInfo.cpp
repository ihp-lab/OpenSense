#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "VideoUsabilityInfo.h"
#include "SequenceParameterSet.h"

namespace HMInterop {
    VideoUsabilityInfo::VideoUsabilityInfo(SequenceParameterSet^ owner, const TComVUI* vui)
        : _owner(owner)
        , _vui(vui) {

        ArgumentNullException::ThrowIfNull(owner, "owner");
        if (!vui) {
            throw gcnew ArgumentNullException("vui");
        }
    }

    bool VideoUsabilityInfo::ColourDescriptionPresent::get() {
        return _vui->getColourDescriptionPresentFlag();
    }

    int VideoUsabilityInfo::ColourPrimaries::get() {
        return _vui->getColourPrimaries();
    }

    int VideoUsabilityInfo::TransferCharacteristics::get() {
        return _vui->getTransferCharacteristics();
    }

    int VideoUsabilityInfo::MatrixCoefficients::get() {
        return _vui->getMatrixCoefficients();
    }

    bool VideoUsabilityInfo::VideoFullRange::get() {
        return _vui->getVideoFullRangeFlag();
    }

    bool VideoUsabilityInfo::AspectRatioInfoPresent::get() {
        return _vui->getAspectRatioInfoPresentFlag();
    }

    int VideoUsabilityInfo::AspectRatioIdc::get() {
        return _vui->getAspectRatioIdc();
    }

    int VideoUsabilityInfo::SarWidth::get() {
        return _vui->getSarWidth();
    }

    int VideoUsabilityInfo::SarHeight::get() {
        return _vui->getSarHeight();
    }

    bool VideoUsabilityInfo::Equals(Object^ other) {
        auto o = dynamic_cast<VideoUsabilityInfo^>(other);
        if (o == nullptr) {
            return false;
        }
        return ColourDescriptionPresent == o->ColourDescriptionPresent
            && ColourPrimaries == o->ColourPrimaries
            && TransferCharacteristics == o->TransferCharacteristics
            && MatrixCoefficients == o->MatrixCoefficients
            && VideoFullRange == o->VideoFullRange
            && AspectRatioInfoPresent == o->AspectRatioInfoPresent
            && AspectRatioIdc == o->AspectRatioIdc
            && SarWidth == o->SarWidth
            && SarHeight == o->SarHeight;
    }

    int VideoUsabilityInfo::GetHashCode() {
        return ColourPrimaries
             ^ (TransferCharacteristics << 8)
             ^ (MatrixCoefficients << 16)
             ^ (AspectRatioIdc << 24)
             ^ SarWidth
             ^ (SarHeight << 16);
    }
}
