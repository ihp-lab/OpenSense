#include "pch.h"

extern "C" {
#include <libavutil/error.h>
}

#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    FFMpegException::FFMpegException(String^ message)
        : Exception(message) {
    }

    FFMpegException::FFMpegException(String^ message, Exception^ innerException)
        : Exception(message, innerException) {
    }

    String^ FFMpegException::DescribeError(int errorCode) {
        char buffer[AV_ERROR_MAX_STRING_SIZE]{};
        av_strerror(errorCode, buffer, sizeof(buffer));
        return String::Format("{0} ({1})", gcnew String(buffer), errorCode);
    }

    FileOpenException::FileOpenException(String^ filename, String^ reason)
        : FFMpegException(String::Format("Cannot open file {0}: {1}", filename, reason)) {
    }

    CodecException::CodecException(String^ message)
        : FFMpegException(message) {
    }
}
