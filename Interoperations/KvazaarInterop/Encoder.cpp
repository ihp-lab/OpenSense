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
        auto dataOut = static_cast<kvz_data_chunk*>(nullptr);
        auto lenOut = uint32_t{0};

        auto result = api->encoder_headers(_encoder, &dataOut, &lenOut);
        if (!result) {
            throw gcnew InvalidOperationException("Failed to get encoder headers");
        }

        if (!dataOut) {
            throw gcnew InvalidOperationException("Encoder headers returned success but no data");
        }

        auto totalLength = static_cast<int>(lenOut);
        return gcnew DataChunk(dataOut, totalLength);
    }

    ValueTuple<DataChunk^, FrameInfo^, Picture^, Picture^> Encoder::Encode(
        Picture^ inputPicture,
        bool noDataChunk,
        bool noFrameInfo,
        bool noSourcePicture,
        bool noReconstructedPicture
    ) {
        ThrowIfDisposed();

        auto api = Api::GetApi();
        auto picIn = inputPicture ? inputPicture->InternalPicture : static_cast<kvz_picture*>(nullptr);
        auto dataOut = static_cast<kvz_data_chunk*>(nullptr);
        auto lenOut = uint32_t{0};
        auto infoOutNative = kvz_frame_info{};
        auto picOutPtr = static_cast<kvz_picture*>(nullptr);
        auto srcOutPtr = static_cast<kvz_picture*>(nullptr);

        auto result = api->encoder_encode(
            _encoder,
            picIn,
            noDataChunk ? nullptr : &dataOut,
            noDataChunk ? nullptr : &lenOut,
            noReconstructedPicture ? nullptr : &picOutPtr,
            noSourcePicture ? nullptr : &srcOutPtr,
            noFrameInfo ? nullptr : &infoOutNative
        );
        if (!result) {
            throw gcnew InvalidOperationException("Encoding failed");
        }

        auto totalLength = static_cast<int>(lenOut);
        auto dataChunk = (!noDataChunk && dataOut) ? gcnew DataChunk(dataOut, totalLength) : nullptr;
        auto frameInfo = (!noFrameInfo && lenOut > 0) ? gcnew FrameInfo(infoOutNative) : nullptr;
        auto sourcePicture = (!noSourcePicture && srcOutPtr) ? gcnew Picture(srcOutPtr) : nullptr;
        auto reconstructedPicture = (!noReconstructedPicture && picOutPtr) ? gcnew Picture(picOutPtr) : nullptr;

        return ValueTuple<DataChunk^, FrameInfo^, Picture^, Picture^>(
            dataChunk, frameInfo, sourcePicture, reconstructedPicture
        );
    }

#pragma region IDisposable

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
#pragma endregion
}