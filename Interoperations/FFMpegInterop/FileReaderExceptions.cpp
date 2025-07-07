#include "pch.h"
#include "FileReaderExceptions.h"

using namespace System;

namespace FFMpegInterop {
    
    FileReaderException::FileReaderException(String^ message) 
        : Exception(message) {
    }

    FileReaderException::FileReaderException(String^ message, Exception^ innerException) 
        : Exception(message, innerException) {
    }

    FileOpenException::FileOpenException(String^ filename) 
        : FileReaderException(String::Format("Cannot open file: {0}", filename)) {
    }

    CodecException::CodecException(String^ message) 
        : FileReaderException(message) {
    }
} 