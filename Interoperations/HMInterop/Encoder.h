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

    public:
        /// <summary>
        /// Creates an encoder with the specified configuration
        /// </summary>
        Encoder([NotNull] EncoderConfig^ config);

        /// <summary>
        /// Encode one frame. May return 0 or more encoded access units.
        /// Returns 0 when the encoder is buffering for GOP reordering.
        /// </summary>
        /// <param name="inputPicture">Input picture to encode</param>
        /// <param name="pts">Presentation timestamp for this frame</param>
        /// <returns>Array of encoded access units</returns>
        [returnvalue: NotNull]
        cli::array<AccessUnitData^>^ Encode([NotNull] PictureYuv^ inputPicture, long long pts);

        /// <summary>
        /// Flush remaining frames. Call after all input is done.
        /// Returns remaining encoded access units.
        /// </summary>
        [returnvalue: NotNull]
        cli::array<AccessUnitData^>^ Flush();

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
