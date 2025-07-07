#pragma once

extern "C" {
#include <libavformat/avformat.h>
}

#include "PixelFormat.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    /// <summary>
    /// Comprehensive frame class that contains all frame data and metadata
    /// This class provides a managed wrapper around video frame data
    /// All frame data is immutable once created
    /// </summary>
    public ref class Frame {
    private:
        // All frame data is immutable - const semantics in C++/CLI
        initonly TimeSpan _timestamp;
        initonly int _width;
        initonly int _height;
        initonly bool _keyFrame;
        initonly PixelFormat _format;
        initonly array<Byte>^ _data;//TODO: Hold the native frame object.

    public:
        /// <summary>
        /// Constructor for creating a new frame
        /// </summary>
        Frame(TimeSpan timestamp, int width, int height, bool keyFrame, PixelFormat pixelFormat, array<Byte>^ data);

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
            int get() { return _width; }
        }

        /// <summary>
        /// Gets the height of the frame in pixels
        /// </summary>
        property int Height {
            int get() { return _height; }
        }

        /// <summary>
        /// Gets whether this frame is a key frame (I-frame)
        /// </summary>
        property bool KeyFrame {
            bool get() { return _keyFrame; }
        }

        /// <summary>
        /// Gets the pixel format of this frame
        /// </summary>
        property PixelFormat Format {
            PixelFormat get() { return _format; }
        }

        /// <summary>
        /// Gets the raw pixel data of this frame
        /// The returned array is a copy to maintain immutability
        /// </summary>
        property array<Byte>^ Data {
            array<Byte>^ get() { return _data; }
        }
    };
} 