#pragma once

#include "PicYuv.h"

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

class TDecTop;

namespace HMInterop {
    /// <summary>
    /// Managed wrapper for TDecTop (HM HEVC decoder).
    /// Supports NAL-unit-at-a-time feeding for use with MP4 demuxer.
    /// </summary>
    public ref class Decoder : IDisposable {
    private:
        TDecTop* _decoder;
        int _pocLastDisplay;

    public:
        Decoder();

        /// <summary>
        /// Feed one NAL unit (without start code or length prefix) to the decoder.
        /// Returns decoded frames (0 or more due to B-frame reordering).
        /// </summary>
        /// <param name="nalData">Raw NAL unit data (without start code or length prefix)</param>
        [returnvalue: NotNull]
        cli::array<PicYuv^>^ FeedNal([NotNull] cli::array<Byte>^ nalData);

        /// <summary>
        /// Signal end of stream, flush remaining B-frames.
        /// Returns all remaining decoded frames.
        /// </summary>
        [returnvalue: NotNull]
        cli::array<PicYuv^>^ FlushAndCollect();

    private:
        /// <summary>
        /// Convert native decoded picture array to managed array.
        /// </summary>
        /// <param name="pics">Pointer to NativeDecodedPic array (passed as void* to avoid native types in header)</param>
        /// <param name="count">Number of pictures</param>
        static cli::array<PicYuv^>^ ConvertDecodedPics(void* pics, int count);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Decoder();
        !Decoder();
#pragma endregion
    };
}
