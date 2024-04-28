#pragma once

using namespace System;

namespace FFMpegInterop {

    public ref class BufferSizeException : public System::Exception {

    public:
        BufferSizeException();

        BufferSizeException(String^ message);

        BufferSizeException(String^ message, Exception^ inner);
    };

}
