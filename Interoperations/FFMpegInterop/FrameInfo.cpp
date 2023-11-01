#include "pch.h"
#include "FrameInfo.h"

namespace FFMpegInterop {
    FrameInfo::FrameInfo(TimeSpan timestamp, int width, int height, bool keyFrame)
        : _timestamp(timestamp), _width(width), _height(height), _KeyFrame(keyFrame) {}
}