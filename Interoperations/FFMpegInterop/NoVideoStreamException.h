#pragma once

using namespace System;

namespace FFMpegInterop {

    public ref class NoVideoStreamException : public System::Exception {

    public:
        NoVideoStreamException();

        NoVideoStreamException(String^ message);

        NoVideoStreamException(String^ message, Exception^ inner);
    };

}
