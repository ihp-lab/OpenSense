#pragma once

extern "C" {
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
}

#include <msclr\marshal_cppstd.h>

#include "PixelFormat.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    /// <summary>
    /// Comprehensive frame class that contains all frame data and metadata
    /// This class provides a managed wrapper around video frame data
    /// Frame now owns the native AVFrame and handles its lifecycle
    /// </summary>
    public ref class Frame : IDisposable {
    private:
        // Frame metadata - immutable once created
        initonly TimeSpan _timestamp;
        
        // Native frame data
        AVFrame* _frame;
        
        // Lazy-loaded byte array
        Lazy<array<Byte>^>^ _lazyData;

        // Private helper methods
        array<Byte>^ CreateByteArray();
        static bool IsMultiPlaneFormat(AVPixelFormat format);
        static int GetVerticalSubsamplingDivisor(AVPixelFormat format);
        static int CalculateMultiPlaneBufferSize(AVFrame* frame, AVPixelFormat format);

    public:
        /// <summary>
        /// Constructor for creating a new frame from native AVFrame
        /// Takes ownership of the AVFrame pointer
        /// </summary>
        Frame(TimeSpan timestamp, AVFrame* frame);

        /// <summary>
        /// Gets the timestamp of this frame relative to the start of the video
        /// </summary>
        property TimeSpan Timestamp {
            TimeSpan get() { return _timestamp; }
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
        /// Gets the raw pixel data of this frame
        /// The data is lazy-loaded when first accessed
        /// </summary>
        property array<Byte>^ Data {
            array<Byte>^ get() { 
                ThrowIfDisposed();
                return _lazyData->Value; 
            }
        }

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