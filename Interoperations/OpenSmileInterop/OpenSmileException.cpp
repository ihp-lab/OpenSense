#include "pch.h"
#include "OpenSmileException.h"

namespace OpenSmileInterop {
    OpenSmileException::OpenSmileException(String^ message)
        : Exception(message) {
    }

    OpenSmileException::OpenSmileException(String^ message, Exception^ innerException)
        : Exception(message, innerException) {
    }
}
