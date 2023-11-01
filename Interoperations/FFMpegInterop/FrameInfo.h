#pragma once

#include <msclr\marshal_cppstd.h>//If moved to the cpp file, there will be compile errors

using namespace System;

namespace FFMpegInterop {
    public value struct FrameInfo {
    private:
        TimeSpan _timestamp;
        int _width;
        int _height;
        bool _KeyFrame;

    public:
        property TimeSpan Timestamp {
            TimeSpan get() { return _timestamp; }
        }

        property int Width {
            int get() { return _width; }
        }

        property int Height {
            int get() { return _height; }
        }

        property bool KeyFrame {
            bool get() { return _KeyFrame; }
        }

        FrameInfo(TimeSpan timestamp, int width, int height, bool keyFrame);
    };
}

