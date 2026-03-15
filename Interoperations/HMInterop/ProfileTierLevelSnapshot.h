#pragma once

#include "Enums.h"

using namespace System;

class TComPTL;

namespace HMInterop {
    /// <summary>
    /// Managed snapshot of HEVC Profile, Tier, and Level (from TComPTL).
    /// </summary>
    public ref class ProfileTierLevelSnapshot sealed {
    private:
        ProfileName _profile;
        LevelName _level;
        LevelTier _tier;
        int _bitDepthConstraint;
        ChromaFormat _chromaFormatConstraint;

    internal:
        ProfileTierLevelSnapshot(const TComPTL* ptl);

    public:
        property ProfileName Profile {
            ProfileName get() { return _profile; }
        }

        property LevelName Level {
            LevelName get() { return _level; }
        }

        property LevelTier Tier {
            LevelTier get() { return _tier; }
        }

        property int BitDepthConstraint {
            int get() { return _bitDepthConstraint; }
        }

        property ChromaFormat ChromaFormatConstraint {
            ChromaFormat get() { return _chromaFormatConstraint; }
        }
    };
}
