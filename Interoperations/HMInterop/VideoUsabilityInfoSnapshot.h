#pragma once

using namespace System;

class TComVUI;

namespace HMInterop {
    /// <summary>
    /// Managed snapshot of HEVC Video Usability Information (from TComVUI).
    /// </summary>
    public ref class VideoUsabilityInfoSnapshot sealed {
    private:
        bool _colourDescriptionPresent;
        int _colourPrimaries;
        int _transferCharacteristics;
        int _matrixCoefficients;
        bool _videoFullRange;
        bool _aspectRatioInfoPresent;
        int _aspectRatioIdc;
        int _sarWidth;
        int _sarHeight;

    internal:
        VideoUsabilityInfoSnapshot(const TComVUI* vui);

    public:
        property bool ColourDescriptionPresent {
            bool get() { return _colourDescriptionPresent; }
        }

        property int ColourPrimaries {
            int get() { return _colourPrimaries; }
        }

        property int TransferCharacteristics {
            int get() { return _transferCharacteristics; }
        }

        property int MatrixCoefficients {
            int get() { return _matrixCoefficients; }
        }

        property bool VideoFullRange {
            bool get() { return _videoFullRange; }
        }

        property bool AspectRatioInfoPresent {
            bool get() { return _aspectRatioInfoPresent; }
        }

        property int AspectRatioIdc {
            int get() { return _aspectRatioIdc; }
        }

        property int SarWidth {
            int get() { return _sarWidth; }
        }

        property int SarHeight {
            int get() { return _sarHeight; }
        }
    };
}
