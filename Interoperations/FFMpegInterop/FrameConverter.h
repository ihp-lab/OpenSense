#pragma once

extern "C" {
#include <libavutil/frame.h>
#include <libswscale/swscale.h>
}

#include "Frame.h"
#include "PixelFormat.h"

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// FrameConverter converts frames to another pixel format and size using libswscale
    /// </summary>
    public ref class FrameConverter sealed : IDisposable {
    private:
        SwsContext* _context;
        AVFrame* _converted;

        // Previous parameters for SwsContext reuse
        int _sourceWidth;
        int _sourceHeight;
        AVPixelFormat _sourceFormat;
        int _sourceColorSpace;
        bool _sourceFullRange;
        int _targetWidth;
        int _targetHeight;
        AVPixelFormat _targetFormat;

    public:
        FrameConverter();

        /// <summary>
        /// Converts a frame into the destination buffer
        /// </summary>
        /// <param name="frame">Frame to convert</param>
        /// <param name="format">Target pixel format, must be a packed format</param>
        /// <param name="width">Target width in pixels</param>
        /// <param name="height">Target height in pixels</param>
        /// <param name="destination">Pointer to the destination buffer</param>
        /// <param name="stride">Stride of the destination buffer in bytes</param>
        void Convert([NotNull] Frame^ frame, PixelFormat format, int width, int height, IntPtr destination, int stride);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~FrameConverter();
        !FrameConverter();
#pragma endregion
    };
}
