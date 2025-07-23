#include "pch.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {
    
    FFMpegException::FFMpegException(String^ message) 
        : Exception(message) {
    }

    FFMpegException::FFMpegException(String^ message, Exception^ innerException) 
        : Exception(message, innerException) {
    }

    FileOpenException::FileOpenException(String^ filename) 
        : FFMpegException(String::Format("Cannot open file: {0}", filename)) {
    }

    CodecException::CodecException(String^ message) 
        : FFMpegException(message) {
    }
} 