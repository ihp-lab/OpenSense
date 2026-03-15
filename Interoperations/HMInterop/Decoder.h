#pragma once

#include "PictureSnapshot.h"
#include <vector>

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

class TDecTop;
struct NativeDecodedPic;

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
        /// Decoded pictures (0 or more due to B-frame reordering) are appended to output.
        /// PictureYuv buffers are obtained from PictureYuvPool.
        /// </summary>
        /// <param name="nalData">Buffer containing NAL unit data</param>
        /// <param name="length">Number of valid bytes in nalData (use -1 for nalData.Length)</param>
        /// <param name="output">List to append decoded pictures to</param>
        void FeedNal([NotNull] cli::array<Byte>^ nalData, int length, [NotNull] System::Collections::Generic::IList<PictureSnapshot^>^ output);

        /// <summary>
        /// Signal end of stream, flush remaining B-frames.
        /// Decoded pictures are appended to output.
        /// </summary>
        void FlushAndCollect([NotNull] System::Collections::Generic::IList<PictureSnapshot^>^ output);

    private:
        /// <summary>
        /// Create PictureSnapshot objects from native decoded pictures and append to output.
        /// Rents PictureYuv from pool and copies pixel data.
        /// </summary>
        static void AppendDecodedPics(const std::vector<NativeDecodedPic>& pics, System::Collections::Generic::IList<PictureSnapshot^>^ output);

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
