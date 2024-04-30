#include "pch.h"
#include "NoVideoStreamException.h"

using namespace System;

namespace FFMpegInterop {
    NoVideoStreamException::NoVideoStreamException() : System::Exception() {
    }

    NoVideoStreamException::NoVideoStreamException(String^ message) : System::Exception(message) {
    }

    NoVideoStreamException::NoVideoStreamException(String^ message, Exception^ inner) : System::Exception(message, inner) {
    }
}

