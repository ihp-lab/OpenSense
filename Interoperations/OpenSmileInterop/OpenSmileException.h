#pragma once

using namespace System;

namespace OpenSmileInterop {
    /// <summary>
    /// Exception thrown when openSMILE fails, e.g. on an invalid configuration or a processing error.
    /// The message is composed of the errors openSMILE logged during the failing call.
    /// </summary>
    public ref class OpenSmileException sealed : public Exception {
    public:
        OpenSmileException(String^ message);
        OpenSmileException(String^ message, Exception^ innerException);
    };
}
