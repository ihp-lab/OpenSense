#include "pch.h"
#include "BufferSizeException.h"

using namespace System;

namespace FFMpegInterop {
    BufferSizeException::BufferSizeException() : System::Exception() {
    }

    BufferSizeException::BufferSizeException(String^ message) : System::Exception(message) {
    }

    BufferSizeException::BufferSizeException(String^ message, Exception^ inner) : System::Exception(message, inner) {
    }
}

