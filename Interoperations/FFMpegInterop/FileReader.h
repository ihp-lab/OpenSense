#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
}

#include <msclr\marshal_cppstd.h>//If moved to the cpp file, there will be compile errors

using namespace System;

namespace FFMpegInterop {
    struct FileReaderUnmanaged;

    public ref class FileReader sealed {
    private:
        FileReaderUnmanaged* const _unmanaged;
        Action<int>^ const _callback;
        int videoStreamIndex = -1;

    public:
        FileReader(String^ filename, Action<int>^ callback);
        void Run();


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

