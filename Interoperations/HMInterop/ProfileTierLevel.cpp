#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/TComSlice.h"
#pragma managed(pop)

#include "ProfileTierLevel.h"
#include "SequenceParameterSet.h"

namespace HMInterop {
    ProfileTierLevel::ProfileTierLevel(SequenceParameterSet^ owner, const TComPTL* ptl)
        : _owner(owner)
        , _ptl(ptl) {

        ArgumentNullException::ThrowIfNull(owner, "owner");
        if (!ptl) {
            throw gcnew ArgumentNullException("ptl");
        }
    }

    ProfileName ProfileTierLevel::Profile::get() {
        return static_cast<ProfileName>(_ptl->getGeneralPTL()->getProfileIdc());
    }

    LevelName ProfileTierLevel::Level::get() {
        return static_cast<LevelName>(_ptl->getGeneralPTL()->getLevelIdc());
    }

    LevelTier ProfileTierLevel::Tier::get() {
        return static_cast<LevelTier>(_ptl->getGeneralPTL()->getTierFlag());
    }

    int ProfileTierLevel::BitDepthConstraint::get() {
        return _ptl->getGeneralPTL()->getBitDepthConstraint();
    }

    ChromaFormat ProfileTierLevel::ChromaFormatConstraint::get() {
        return static_cast<ChromaFormat>(_ptl->getGeneralPTL()->getChromaFormatConstraint());
    }

    bool ProfileTierLevel::Equals(Object^ other) {
        auto o = dynamic_cast<ProfileTierLevel^>(other);
        if (o == nullptr) {
            return false;
        }
        return Profile == o->Profile
            && Level == o->Level
            && Tier == o->Tier
            && BitDepthConstraint == o->BitDepthConstraint
            && ChromaFormatConstraint == o->ChromaFormatConstraint;
    }

    int ProfileTierLevel::GetHashCode() {
        return static_cast<int>(Profile)
             ^ (static_cast<int>(Level) << 8)
             ^ (static_cast<int>(Tier) << 16)
             ^ (BitDepthConstraint << 20)
             ^ (static_cast<int>(ChromaFormatConstraint) << 24);
    }
}
