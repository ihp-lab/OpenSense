#pragma once

extern "C" {
#include <libavutil/frame.h>
}

#include "FrameFlags.h"
#include "PixelFormat.h"

using namespace System;
using namespace System::Runtime::CompilerServices;

namespace FFMpegInterop {
    /// <summary>
    /// Comprehensive frame class that contains all frame data and metadata
    /// This class provides a managed wrapper around video frame data
    /// Frame now owns the native AVFrame and handles its lifecycle
    /// </summary>
    public ref class Frame sealed : IDisposable {
    private:
        AVFrame* _frame;

    internal:
        /// <summary>
        /// Constructor for creating a new frame from native AVFrame
        /// Takes ownership of the AVFrame pointer, its pts must be in .NET Ticks
        /// </summary>
        Frame(AVFrame* frame);

        /// <summary>
        /// Gets the internal AVFrame pointer for direct FFmpeg operations
        /// </summary>
        property AVFrame* InternalFrame {
            AVFrame* get();
        }

    public:
        /// <summary>
        /// Presentation (display) time offset relative to the start of the file, in .NET Ticks.
        /// </summary>
        property long long PresentationTimeOffset {
            long long get();
        }

        /// <summary>
        /// Gets the width of the frame in pixels
        /// </summary>
        property int Width {
            int get();
        }

        /// <summary>
        /// Gets the height of the frame in pixels
        /// </summary>
        property int Height {
            int get();
        }

        /// <summary>
        /// Gets the pixel format of this frame
        /// </summary>
        property PixelFormat Format {
            PixelFormat get();
        }

        /// <summary>
        /// Gets the flags of the frame
        /// </summary>
        property FrameFlags Flags {
            FrameFlags get();
        }

        /// <summary>
        /// Gets the number of data planes in this frame
        /// </summary>
        property int PlaneCount {
            int get();
        }

        /// <summary>
        /// Gets buffer information for a specific plane
        /// </summary>
        /// <param name="planeIndex">The zero-based index of the plane</param>
        /// <returns>A tuple containing buffer pointer, stride, and length</returns>
        [returnvalue: TupleElementNames(gcnew array<String^>{"Data", "Stride", "Length"})]
        ValueTuple<IntPtr, int, int> GetPlaneBuffer(int planeIndex);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Frame();
        !Frame();
#pragma endregion
    };
}
