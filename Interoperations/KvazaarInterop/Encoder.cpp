#include "pch.h"
#include "Encoder.h"

using namespace System;
using namespace System::Threading;

namespace KvazaarInterop {
    Encoder::Encoder([NotNull] Config^ config)
        : _encoder(nullptr)
        , _disposed(false) {

        ArgumentNullException::ThrowIfNull(config, "config");

        // Serialize encoder_open: Kvazaar's kvz_strategyselector_init() modifies
        // unprotected global state during encoder creation.
        Monitor::Enter(s_lock);
        try {
            auto api = Api::GetApi();
            _encoder = api->encoder_open(config->InternalConfig);
        } finally {
            Monitor::Exit(s_lock);
        }

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
        return gcnew DataChunk(dataOut, totalLength, 0LL); // Headers have no PTS
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

        // Always request srcOutPtr to extract PTS for DataChunk, regardless of noSourcePicture.
        // noSourcePicture only controls whether the managed Picture wrapper is returned to the caller.
        auto result = api->encoder_encode(
            _encoder,
            picIn,
            noDataChunk ? nullptr : &dataOut,
            noDataChunk ? nullptr : &lenOut,
            noReconstructedPicture ? nullptr : &picOutPtr,
            &srcOutPtr,
            noFrameInfo ? nullptr : &infoOutNative
        );
        if (!result) {
            throw gcnew InvalidOperationException("Encoding failed");
        }

        auto totalLength = static_cast<int>(lenOut);
        auto pts = srcOutPtr ? srcOutPtr->pts : 0LL;
        auto dataChunk = (!noDataChunk && dataOut) ? gcnew DataChunk(dataOut, totalLength, pts) : nullptr;
        auto frameInfo = (!noFrameInfo && lenOut > 0) ? gcnew FrameInfo(infoOutNative) : nullptr;
        Picture^ sourcePicture = nullptr;
        if (srcOutPtr) {
            if (noSourcePicture) {
                api->picture_free(srcOutPtr);
            } else {
                sourcePicture = gcnew Picture(srcOutPtr);
            }
        }
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

        // Serialize encoder_close under the same lock as encoder_open,
        // in case encoder_close touches global state in future Kvazaar versions.
        Monitor::Enter(s_lock);
        try {
            this->!Encoder();
        } finally {
            Monitor::Exit(s_lock);
        }
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