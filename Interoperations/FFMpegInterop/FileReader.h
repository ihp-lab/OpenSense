#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
}

#include <msclr\marshal_cppstd.h>//If moved to the cpp file, there will be compile errors

#include "FrameInfo.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    struct FileReaderUnmanaged;

    public ref class FileReader sealed {
    private:
        FileReaderUnmanaged* const _unmanaged;
        AVPixelFormat targetFormat = AVPixelFormat::AV_PIX_FMT_RGB24;
        int videoStreamIndex = -1;
        double timeBase;
        int prevWidth = -1;
        int prevHeight = -1;

    public:
        FileReader(String^ filename);
        property int TargetFormat {
            int get() { return targetFormat; }
            void set(int value) { targetFormat = static_cast<AVPixelFormat>(value); }
        }
        void ReadOneFrame(Func<FrameInfo, ValueTuple<IntPtr, int>>^ allocator, [Out] bool% success, [Out] bool% eof);

    private:
        static int ComputeBufferSize(const AVFrame& frame);


#pragma region IDisposable
    private:
        bool disposed;
    public:
        ~FileReader();
        !FileReader();
#pragma endregion

    };

#pragma region Classes
    struct FileReaderUnmanaged {
        AVFormatContext* formatContext = nullptr;
        AVCodecContext* codecContext = nullptr;
        AVPacket* packet = nullptr;
        SwsContext* swsCtx = nullptr;
        AVFrame* rawFrame = nullptr;
        AVFrame* convertedFrame = nullptr;
    };
#pragma endregion

}

