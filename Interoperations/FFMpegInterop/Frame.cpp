#include "pch.h"

#include "Frame.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    
    Frame::Frame(TimeSpan timestamp, int width, int height, bool keyFrame, PixelFormat format, array<Byte>^ data)
        : _timestamp(timestamp)
        , _width(width)
        , _height(height)
        , _keyFrame(keyFrame)
        , _format(format)
        , _data(data) {
        
        // Validate input parameters
        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }
        if (data->Length == 0) {
            throw gcnew ArgumentException("Data array cannot be empty", "data");
        }
    }
} 