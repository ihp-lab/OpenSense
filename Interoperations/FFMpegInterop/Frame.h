#pragma once

extern "C" {
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
}

#include <msclr\marshal_cppstd.h>

#include "PixelFormat.h"

using namespace System;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// Comprehensive frame class that contains all frame data and metadata
    /// This class provides a managed wrapper around video frame data
    /// Frame now owns the native AVFrame and handles its lifecycle
    /// </summary>
    public ref class Frame : IDisposable {
    private:

        // Native frame data
        AVFrame* _frame;

        // Private helper methods
        static bool IsMultiPlaneFormat(AVPixelFormat format);
        static int GetVerticalSubsamplingDivisor(AVPixelFormat format);
        static int CalculateBufferSize(AVFrame* frame, int planeIndex);
        static int CalculateBufferSize(AVFrame* frame);
        static void CopyDataToFrame(byte* data, int length, AVFrame* frame);

    public:
        /// <summary>
        /// Constructor for creating a new frame from native AVFrame
        /// Takes ownership of the AVFrame pointer
        /// </summary>
        Frame(AVFrame* frame);

        /// <summary>
        /// Constructor for creating a new frame from managed data
        /// Creates a new AVFrame and copies data from the provided array
        /// </summary>
        Frame(long long pts, int width, int height, PixelFormat format, [NotNull] IntPtr data, int length);

        /// <summary>
        /// Constructor for creating a new frame that shares data buffers with an existing frame but has a different PTS
        /// This creates a shallow copy where the new frame references the same pixel data as the source frame
        /// but can have different timing information. Uses FFmpeg's reference counting for safe memory management.
        /// </summary>
        /// <param name="pts">The presentation timestamp for the new frame</param>
        /// <param name="frame">The source frame to share data buffers with</param>
        Frame(long long pts, [NotNull] Frame^ frame);

        /// <summary>
        /// Gets the internal AVFrame pointer for direct FFmpeg operations
        /// WARNING: This provides direct access to the internal frame data.
        /// The caller must not free this pointer or modify frame properties that could affect lifecycle.
        /// </summary>
        /// <returns>Pointer to the internal AVFrame</returns>
        property AVFrame* InternalAVFrame {
            AVFrame* get() {
                ThrowIfDisposed();
                return _frame;
            }
        }

        /// <summary>
        /// Gets the raw presentation timestamp (PTS) value in the frame's native time base
        /// This is useful for variable frame rate encoding where precise timing is needed
        /// </summary>
        property long long PTS {
            long long get() {
                ThrowIfDisposed();
                return _frame->pts != AV_NOPTS_VALUE ? _frame->pts : _frame->best_effort_timestamp;
            }
        }

        /// <summary>
        /// Gets the timestamp of this frame relative to the start of the video
        /// </summary>
        property TimeSpan Timestamp {
            TimeSpan get() { return TimeSpan::FromSeconds(PTS * av_q2d(_frame->time_base)); }
        }

        /// <summary>
        /// Gets the width of the frame in pixels
        /// </summary>
        property int Width {
            int get() { 
                ThrowIfDisposed();
                return _frame->width; 
            }
        }

        /// <summary>
        /// Gets the height of the frame in pixels
        /// </summary>
        property int Height {
            int get() { 
                ThrowIfDisposed();
                return _frame->height; 
            }
        }

        /// <summary>
        /// Gets whether this frame is a key frame (I-frame)
        /// </summary>
        property bool KeyFrame {
            bool get() { 
                ThrowIfDisposed();
                return _frame->key_frame; 
            }
        }

        /// <summary>
        /// Gets the pixel format of this frame
        /// </summary>
        property PixelFormat Format {
            PixelFormat get() { 
                ThrowIfDisposed();
                return static_cast<PixelFormat>(_frame->format); 
            }
        }

        /// <summary>
        /// Gets the number of data planes in this frame
        /// </summary>
        property int PlaneCount {
            int get() { 
                ThrowIfDisposed();
                return IsMultiPlaneFormat(static_cast<AVPixelFormat>(_frame->format)) ? 3 : 1; 
            }
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

        /// <summary>
        /// Throws ObjectDisposedException if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();

    public:
        ~Frame();
        !Frame();
#pragma endregion
    };
} 