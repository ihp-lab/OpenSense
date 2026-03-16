#pragma once

#include "EncoderConfig.h"
#include "PictureYuv.h"
#include "AccessUnitData.h"

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

class TEncTop;

namespace HMInterop {
    /// <summary>
    /// Managed wrapper for TEncTop (HM HEVC encoder)
    /// </summary>
    public ref class Encoder : IDisposable {
    private:
        EncoderConfig^ _config;
        TEncTop* _encoder;

        // POC -> PTS mapping for B-frame timestamp recovery (std::map<int, long long>*)
        void* _ptsMap;
        int _inputFrameCount;

        // Reconstructed picture list (TComList<TComPicYuv*>*)
        void* _recPicList;

        // CTU parameters for HMContext
        int _maxWidth;
        int _maxHeight;
        int _maxDepth;

    public:
        /// <summary>
        /// Creates an encoder with the specified configuration
        /// </summary>
        Encoder([NotNull] EncoderConfig^ config);

        /// <summary>
        /// Encode one frame. Encoded access units (0 or more) are appended to output.
        /// Output is empty when the encoder is buffering for GOP reordering.
        /// </summary>
        void Encode([NotNull] PictureYuv^ inputPicture, long long pts, [NotNull] System::Collections::Generic::IList<AccessUnitData^>^ output);

        /// <summary>
        /// Flush remaining frames. Call after all input is done.
        /// Encoded access units are appended to output.
        /// </summary>
        void Flush([NotNull] System::Collections::Generic::IList<AccessUnitData^>^ output);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Encoder();
        !Encoder();
#pragma endregion
    };
}
