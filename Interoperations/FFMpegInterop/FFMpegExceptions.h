#pragma once

using namespace System;

namespace FFMpegInterop {
    
    /// <summary>
    /// Base exception class for FFMpeg operations
    /// </summary>
    public ref class FFMpegException : public Exception {
    public:
        FFMpegException(String^ message);
        FFMpegException(String^ message, Exception^ innerException);
    };

    /// <summary>
    /// Exception thrown when a file cannot be opened
    /// </summary>
    public ref class FileOpenException : public FFMpegException {
    public:
        FileOpenException(String^ filename);
    };

    /// <summary>
    /// Exception thrown when codec operations fail
    /// </summary>
    public ref class CodecException : public FFMpegException {
    public:
        CodecException(String^ message);
    };
} 