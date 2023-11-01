#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
}

#include <msclr\marshal_cppstd.h>//If moved to the cpp file, there will be compile errors

#include "FrameInfo.h"

using namespace System;

namespace FFMpegInterop {
    struct FileReaderUnmanaged;

    public ref class FileReader sealed {
    private:
        FileReaderUnmanaged* const _unmanaged;
        Func<FrameInfo, ValueTuple<IntPtr, int, Action^>>^ const _callback;
        int videoStreamIndex = -1;
        AVPixelFormat const _targetPixelFormat = AVPixelFormat::AV_PIX_FMT_BGR24;

    public:
        FileReader(String^ filename, Func<FrameInfo, ValueTuple<IntPtr, int, Action^>>^ callback);
        void Run();

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
    };
#pragma endregion

}

