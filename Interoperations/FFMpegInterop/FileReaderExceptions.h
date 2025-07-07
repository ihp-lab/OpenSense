#pragma once

using namespace System;

namespace FFMpegInterop {
    
    /// <summary>
    /// Base exception class for FileReader operations
    /// </summary>
    public ref class FileReaderException : public Exception {
    public:
        FileReaderException(String^ message);
        FileReaderException(String^ message, Exception^ innerException);
    };

    /// <summary>
    /// Exception thrown when a file cannot be opened
    /// </summary>
    public ref class FileOpenException : public FileReaderException {
    public:
        FileOpenException(String^ filename);
    };

    /// <summary>
    /// Exception thrown when codec operations fail
    /// </summary>
    public ref class CodecException : public FileReaderException {
    public:
        CodecException(String^ message);
    };
} 