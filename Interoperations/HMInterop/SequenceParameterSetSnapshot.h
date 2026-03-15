#pragma once

#include "BitDepths.h"
#include "Enums.h"
#include "ProfileTierLevelSnapshot.h"
#include "VideoUsabilityInfoSnapshot.h"

using namespace System;

class TComSPS;

namespace HMInterop {
    /// <summary>
    /// Managed snapshot of HEVC Sequence Parameter Set (from TComSPS).
    /// </summary>
    public ref class SequenceParameterSetSnapshot sealed {
    private:
        int _spsId;
        BitDepths _bitDepths;
        ChromaFormat _chromaFormatIdc;
        int _picWidthInLumaSamples;
        int _picHeightInLumaSamples;
        ProfileTierLevelSnapshot^ _profileTierLevel;
        VideoUsabilityInfoSnapshot^ _videoUsabilityInfo;
        int _maxCUWidth;
        int _maxCUHeight;
        int _maxTLayers;

    internal:
        SequenceParameterSetSnapshot(const TComSPS* sps);

    public:
        /// <summary>
        /// Create a minimal SPS from managed parameters (for non-decoder scenarios).
        /// ProfileTierLevel and VUI will be nullptr.
        /// </summary>
        SequenceParameterSetSnapshot(HMInterop::BitDepths bitDepths, ChromaFormat chromaFormatIdc, int width, int height);

        property int SpsId {
            int get() { return _spsId; }
        }

        property BitDepths BitDepths {
            HMInterop::BitDepths get() { return _bitDepths; }
        }

        property ChromaFormat ChromaFormatIdc {
            ChromaFormat get() { return _chromaFormatIdc; }
        }

        property int PicWidthInLumaSamples {
            int get() { return _picWidthInLumaSamples; }
        }

        property int PicHeightInLumaSamples {
            int get() { return _picHeightInLumaSamples; }
        }

        property ProfileTierLevelSnapshot^ ProfileTierLevelSnapshot {
            HMInterop::ProfileTierLevelSnapshot^ get() { return _profileTierLevel; }
        }

        property VideoUsabilityInfoSnapshot^ VideoUsabilityInfoSnapshot {
            HMInterop::VideoUsabilityInfoSnapshot^ get() { return _videoUsabilityInfo; }
        }

        property int MaxCUWidth {
            int get() { return _maxCUWidth; }
        }

        property int MaxCUHeight {
            int get() { return _maxCUHeight; }
        }

        property int MaxTLayers {
            int get() { return _maxTLayers; }
        }
    };
}
