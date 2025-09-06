#include "pch.h"
#include "Encoder.h"

using namespace System;

namespace KvazaarInterop {
    Encoder::Encoder([NotNull] Config^ config)
        : _encoder(nullptr)
        , _disposed(false) {
        
        ArgumentNullException::ThrowIfNull(config, "config");
        
        auto api = Api::GetApi();
        _encoder = api->encoder_open(config->InternalConfig);
        
        if (!_encoder) {
            throw gcnew InvalidOperationException("Failed to create encoder");
        }
    }

    DataChunk^ Encoder::GetHeaders() {
        ThrowIfDisposed();
        
        auto api = Api::GetApi();
        kvz_data_chunk* dataOut = nullptr;
        uint32_t lenOut = 0;
        
        auto result = api->encoder_headers(_encoder, &dataOut, &lenOut);
        if (!result) {
            throw gcnew InvalidOperationException("Failed to get encoder headers");
        }
        
        if (!dataOut) {
            return nullptr;
        }
        
        return gcnew DataChunk(dataOut);
    }

    DataChunk^ Encoder::GetHeaders([Out] int% length) {
        ThrowIfDisposed();
        
        auto api = Api::GetApi();
        kvz_data_chunk* dataOut = nullptr;
        uint32_t lenOut = 0;
        
        auto result = api->encoder_headers(_encoder, &dataOut, &lenOut);
        if (!result) {
            throw gcnew InvalidOperationException("Failed to get encoder headers");
        }
        
        length = static_cast<int>(lenOut);
        
        if (!dataOut) {
            return nullptr;
        }
        
        return gcnew DataChunk(dataOut);
    }

    DataChunk^ Encoder::Encode(
        Picture^ pictureIn, 
        [Out] Picture^% pictureOut, 
        [Out] Picture^% sourceOut,
        [Out] FrameInfo^% infoOut
    ) {
        ThrowIfDisposed();
        
        auto api = Api::GetApi();
        kvz_picture* picIn = pictureIn ? pictureIn->InternalPicture : nullptr;
        kvz_data_chunk* dataOut = nullptr;
        uint32_t lenOut = 0;
        kvz_picture* picOut = nullptr;
        kvz_picture* srcOut = nullptr;
        kvz_frame_info infoOutNative;
        
        auto result = api->encoder_encode(_encoder, picIn, &dataOut, &lenOut, &picOut, &srcOut, &infoOutNative);
        if (!result) {
            throw gcnew InvalidOperationException("Encoding failed");
        }
        
        pictureOut = nullptr;
        sourceOut = nullptr;
        infoOut = nullptr;
        
        if (picOut) {
            pictureOut = gcnew Picture(picOut->width, picOut->height);
            delete pictureOut;
            pictureOut = nullptr;
        }
        
        if (srcOut) {
            sourceOut = gcnew Picture(srcOut->width, srcOut->height);
            delete sourceOut;
            sourceOut = nullptr;
        }
        
        if (lenOut > 0) {
            infoOut = gcnew FrameInfo(infoOutNative);
        }
        
        if (!dataOut) {
            return nullptr;
        }
        
        return gcnew DataChunk(dataOut);
    }

    DataChunk^ Encoder::Encode(
        Picture^ pictureIn,
        [Out] int% length,
        [Out] Picture^% pictureOut,
        [Out] Picture^% sourceOut,
        [Out] FrameInfo^% infoOut
    ) {
        ThrowIfDisposed();
        
        auto api = Api::GetApi();
        kvz_picture* picIn = pictureIn ? pictureIn->InternalPicture : nullptr;
        kvz_data_chunk* dataOut = nullptr;
        uint32_t lenOut = 0;
        kvz_picture* picOut = nullptr;
        kvz_picture* srcOut = nullptr;
        kvz_frame_info infoOutNative;
        
        auto result = api->encoder_encode(_encoder, picIn, &dataOut, &lenOut, &picOut, &srcOut, &infoOutNative);
        if (!result) {
            throw gcnew InvalidOperationException("Encoding failed");
        }
        
        length = static_cast<int>(lenOut);
        pictureOut = nullptr;
        sourceOut = nullptr;
        infoOut = nullptr;
        
        if (picOut) {
            pictureOut = gcnew Picture(picOut->width, picOut->height);
            delete pictureOut;
            pictureOut = nullptr;
        }
        
        if (srcOut) {
            sourceOut = gcnew Picture(srcOut->width, srcOut->height);
            delete sourceOut;
            sourceOut = nullptr;
        }
        
        if (lenOut > 0) {
            infoOut = gcnew FrameInfo(infoOutNative);
        }
        
        if (!dataOut) {
            return nullptr;
        }
        
        return gcnew DataChunk(dataOut);
    }

    DataChunk^ Encoder::Encode(Picture^ pictureIn) {
        ThrowIfDisposed();
        
        auto api = Api::GetApi();
        kvz_picture* picIn = pictureIn ? pictureIn->InternalPicture : nullptr;
        kvz_data_chunk* dataOut = nullptr;
        uint32_t lenOut = 0;
        
        auto result = api->encoder_encode(_encoder, picIn, &dataOut, &lenOut, nullptr, nullptr, nullptr);
        if (!result) {
            throw gcnew InvalidOperationException("Encoding failed");
        }
        
        if (!dataOut) {
            return nullptr;
        }
        
        return gcnew DataChunk(dataOut);
    }

    void Encoder::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Encoder::~Encoder() {
        if (_disposed) {
            return;
        }

        this->!Encoder();
        _disposed = true;
    }

    Encoder::!Encoder() {
        if (_encoder) {
            auto api = Api::GetApi();
            api->encoder_close(_encoder);
            _encoder = nullptr;
        }
    }
}