#pragma once

#include "Picture.h"
#include "AccessUnit.h"
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

        // SPS cache: reuse the same managed object when the native SPS pointer hasn't changed.
        SequenceParameterSet^ _lastSps;

        // CTU parameters for HMContext (updated from SPS after first decode)
        int _maxWidth;
        int _maxHeight;
        int _maxDepth;

    public:
        Decoder();

        /// <summary>
        /// Feed all NAL units from an AccessUnit to the decoder in a single pin.
        /// Decoded pictures (0 or more due to B-frame reordering) are appended to output.
        /// PictureYuv buffers are obtained from PictureYuvPool.
        /// </summary>
        void FeedAccessUnit([NotNull] AccessUnit^ accessUnit, [NotNull] System::Collections::Generic::IList<Picture^>^ output);

        /// <summary>
        /// Signal end of stream, flush remaining B-frames.
        /// Decoded pictures are appended to output.
        /// </summary>
        void FlushAndCollect([NotNull] System::Collections::Generic::IList<Picture^>^ output);

    private:
        /// <summary>
        /// Create Picture objects from native decoded pictures and append to output.
        /// Rents PictureYuv from pool and copies pixel data.
        /// </summary>
        void AppendDecodedPics(const std::vector<NativeDecodedPic>& pics, System::Collections::Generic::IList<Picture^>^ output);

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
