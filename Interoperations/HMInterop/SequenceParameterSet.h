#pragma once

#include "BitDepths.h"
#include "Enums.h"
#include "ProfileTierLevel.h"
#include "VideoUsabilityInfo.h"

using namespace System;

class TComSPS;

namespace HMInterop {
    /// <summary>
    /// Managed wrapper for HEVC Sequence Parameter Set (from TComSPS).
    /// Owns a deep copy of the native TComSPS object.
    /// </summary>
    public ref class SequenceParameterSet sealed : IDisposable {
    private:
        TComSPS* _sps;
        const void* _sourcePtr;
        HMInterop::ProfileTierLevel^ _profileTierLevel;
        HMInterop::VideoUsabilityInfo^ _videoUsabilityInfo;

    internal:
        SequenceParameterSet(const TComSPS* source);

    public:
        /// <summary>
        /// Create a minimal SPS from managed parameters (for non-decoder scenarios).
        /// ProfileTierLevel and VUI will be nullptr.
        /// </summary>
        SequenceParameterSet(HMInterop::BitDepths bitDepths, ChromaFormat chromaFormatIdc, int width, int height);

        property int SpsId {
            int get();
        }

        property HMInterop::BitDepths BitDepths {
            HMInterop::BitDepths get();
        }

        property ChromaFormat ChromaFormatIdc {
            ChromaFormat get();
        }

        property int PicWidthInLumaSamples {
            int get();
        }

        property int PicHeightInLumaSamples {
            int get();
        }

        property HMInterop::ProfileTierLevel^ ProfileTierLevel {
            HMInterop::ProfileTierLevel^ get();
        }

        property HMInterop::VideoUsabilityInfo^ VideoUsabilityInfo {
            HMInterop::VideoUsabilityInfo^ get();
        }

        property int MaxCUWidth {
            int get();
        }

        property int MaxCUHeight {
            int get();
        }

        property int MaxTotalCUDepth {
            int get();
        }

        property int MaxTLayers {
            int get();
        }

        virtual bool Equals(Object^ other) override;
        virtual int GetHashCode() override;

    internal:
        property TComSPS* InternalSps {
            TComSPS* get();
        }

        property const void* SourcePtr {
            const void* get();
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~SequenceParameterSet();
        !SequenceParameterSet();
#pragma endregion
    };
}
