#pragma once

#include "Enums.h"

using namespace System;

class TComPTL;

namespace HMInterop {

    ref class SequenceParameterSet;

    /// <summary>
    /// Managed wrapper for HEVC Profile, Tier, and Level (from TComPTL).
    /// Holds a pointer into the owning SequenceParameterSet's native TComSPS.
    /// </summary>
    public ref class ProfileTierLevel sealed {
    private:
        SequenceParameterSet^ _owner;
        const TComPTL* _ptl;

    internal:
        ProfileTierLevel(SequenceParameterSet^ owner, const TComPTL* ptl);

    public:
        property ProfileName Profile {
            ProfileName get();
        }

        property LevelName Level {
            LevelName get();
        }

        property LevelTier Tier {
            LevelTier get();
        }

        property int BitDepthConstraint {
            int get();
        }

        property ChromaFormat ChromaFormatConstraint {
            ChromaFormat get();
        }

        virtual bool Equals(Object^ other) override;
        virtual int GetHashCode() override;
    };
}
