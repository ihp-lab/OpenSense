#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "ProfileTierLevelSnapshot.h"

namespace HMInterop {
    ProfileTierLevelSnapshot::ProfileTierLevelSnapshot(const TComPTL* ptl)
        : _profile(static_cast<ProfileName>(ptl->getGeneralPTL()->getProfileIdc()))
        , _level(static_cast<LevelName>(ptl->getGeneralPTL()->getLevelIdc()))
        , _tier(static_cast<LevelTier>(ptl->getGeneralPTL()->getTierFlag()))
        , _bitDepthConstraint(ptl->getGeneralPTL()->getBitDepthConstraint())
        , _chromaFormatConstraint(static_cast<ChromaFormat>(ptl->getGeneralPTL()->getChromaFormatConstraint())) {
    }
}
