#pragma once

using namespace System;

class TComVUI;

namespace HMInterop {

    ref class SequenceParameterSet;

    /// <summary>
    /// Managed wrapper for HEVC Video Usability Information (from TComVUI).
    /// Holds a pointer into the owning SequenceParameterSet's native TComSPS.
    /// </summary>
    public ref class VideoUsabilityInfo sealed {
    private:
        SequenceParameterSet^ _owner;
        const TComVUI* _vui;

    internal:
        VideoUsabilityInfo(SequenceParameterSet^ owner, const TComVUI* vui);

    public:
        property bool ColourDescriptionPresent {
            bool get();
        }

        property int ColourPrimaries {
            int get();
        }

        property int TransferCharacteristics {
            int get();
        }

        property int MatrixCoefficients {
            int get();
        }

        property bool VideoFullRange {
            bool get();
        }

        property bool AspectRatioInfoPresent {
            bool get();
        }

        property int AspectRatioIdc {
            int get();
        }

        property int SarWidth {
            int get();
        }

        property int SarHeight {
            int get();
        }

        virtual bool Equals(Object^ other) override;
        virtual int GetHashCode() override;
    };
}
