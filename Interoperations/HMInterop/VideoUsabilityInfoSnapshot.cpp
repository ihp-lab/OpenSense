#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "VideoUsabilityInfoSnapshot.h"

namespace HMInterop {
    VideoUsabilityInfoSnapshot::VideoUsabilityInfoSnapshot(const TComVUI* vui)
        : _colourDescriptionPresent(vui->getColourDescriptionPresentFlag())
        , _colourPrimaries(vui->getColourPrimaries())
        , _transferCharacteristics(vui->getTransferCharacteristics())
        , _matrixCoefficients(vui->getMatrixCoefficients())
        , _videoFullRange(vui->getVideoFullRangeFlag())
        , _aspectRatioInfoPresent(vui->getAspectRatioInfoPresentFlag())
        , _aspectRatioIdc(vui->getAspectRatioIdc())
        , _sarWidth(vui->getSarWidth())
        , _sarHeight(vui->getSarHeight()) {
    }
}
