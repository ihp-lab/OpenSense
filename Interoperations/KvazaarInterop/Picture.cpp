#include "pch.h"
#include "Picture.h"

#include <cstring>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace KvazaarInterop {
    Picture::Picture(kvz_picture* picture)
        : _picture(picture)
        , _disposed(false) {

        if (!picture) {
            throw gcnew ArgumentNullException("picture");
        }
    }

    Picture::Picture(KvazaarInterop::ChromaFormat chromaFormat, int width, int height)
        : _picture(nullptr)
        , _disposed(false) {

        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }

        auto api = Api::GetApi();
        auto kvzChromaFormat = static_cast<kvz_chroma_format>(chromaFormat);
        _picture = api->picture_alloc_csp(kvzChromaFormat, width, height);

        if (!_picture) {
            throw gcnew OutOfMemoryException("Failed to allocate picture with chroma format");
        }
    }

    void Picture::CopyYPlane(IntPtr data, int length) {
        ThrowIfDisposed();

        if (data == IntPtr::Zero) {
            throw gcnew ArgumentNullException("data");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }

        // Calculate Y plane size in bytes
        // Note: stride and dimensions are in pixels, but for 16-bit mode each pixel is 2 bytes
        auto planeSizeInPixels = _picture->stride * _picture->height;
        auto planeSizeInBytes = planeSizeInPixels * sizeof(kvz_pixel);
        if (length > planeSizeInBytes) {
            throw gcnew ArgumentException(String::Format(
                "Data length ({0} bytes) exceeds Y plane size ({1} bytes). Picture dimensions: {2}x{3}, stride: {4} pixels, pixel size: {5} bytes",
                length, planeSizeInBytes, _picture->width, _picture->height, _picture->stride, sizeof(kvz_pixel)
            ));
        }

        memcpy(_picture->y, data.ToPointer(), length);
    }

    void Picture::CopyUPlane(IntPtr data, int length) {
        ThrowIfDisposed();

        if (data == IntPtr::Zero) {
            throw gcnew ArgumentNullException("data");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }

        if (!_picture->u) {
            throw gcnew InvalidOperationException("Picture has no U plane");
        }

        // Calculate U plane size based on chroma format
        auto yPlaneSizeInPixels = _picture->stride * _picture->height;
        auto chromaFormat = static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);
        auto uPlaneSizeInBytes = CalculateChromaPlaneSizeInBytes(yPlaneSizeInPixels, chromaFormat);
        if (length > uPlaneSizeInBytes) {
            throw gcnew ArgumentException(String::Format(
                "Data length ({0} bytes) exceeds U plane size ({1} bytes). Picture dimensions: {2}x{3}, stride: {4}, chroma format: {5}",
                length, uPlaneSizeInBytes, _picture->width, _picture->height, _picture->stride, chromaFormat
            ));
        }

        memcpy(_picture->u, data.ToPointer(), length);
    }

    void Picture::CopyVPlane(IntPtr data, int length) {
        ThrowIfDisposed();

        if (data == IntPtr::Zero) {
            throw gcnew ArgumentNullException("data");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }

        if (!_picture->v) {
            throw gcnew InvalidOperationException("Picture has no V plane");
        }

        // Calculate V plane size based on chroma format
        auto yPlaneSizeInPixels = _picture->stride * _picture->height;
        auto chromaFormat = static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);
        auto vPlaneSizeInBytes = CalculateChromaPlaneSizeInBytes(yPlaneSizeInPixels, chromaFormat);
        if (length > vPlaneSizeInBytes) {
            throw gcnew ArgumentException(String::Format(
                "Data length ({0} bytes) exceeds V plane size ({1} bytes). Picture dimensions: {2}x{3}, stride: {4}, chroma format: {5}",
                length, vPlaneSizeInBytes, _picture->width, _picture->height, _picture->stride, chromaFormat
            ));
        }

        memcpy(_picture->v, data.ToPointer(), length);
    }

    ValueTuple<IntPtr, int> Picture::GetYPlane() {
        ThrowIfDisposed();

        auto planeSizeInPixels = _picture->stride * _picture->height;
        auto planeSizeInBytes = planeSizeInPixels * sizeof(kvz_pixel);
        return ValueTuple<IntPtr, int>(IntPtr(_picture->y), planeSizeInBytes);
    }

    ValueTuple<IntPtr, int> Picture::GetUPlane() {
        ThrowIfDisposed();

        if (!_picture->u) {
            return ValueTuple<IntPtr, int>(IntPtr::Zero, 0);
        }

        auto yPlaneSizeInPixels = _picture->stride * _picture->height;
        auto chromaFormat = static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);
        auto planeSizeInBytes = CalculateChromaPlaneSizeInBytes(yPlaneSizeInPixels, chromaFormat);
        return ValueTuple<IntPtr, int>(IntPtr(_picture->u), planeSizeInBytes);
    }

    ValueTuple<IntPtr, int> Picture::GetVPlane() {
        ThrowIfDisposed();

        if (!_picture->v) {
            return ValueTuple<IntPtr, int>(IntPtr::Zero, 0);
        }

        auto yPlaneSizeInPixels = _picture->stride * _picture->height;
        auto chromaFormat = static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);
        auto planeSizeInBytes = CalculateChromaPlaneSizeInBytes(yPlaneSizeInPixels, chromaFormat);
        return ValueTuple<IntPtr, int>(IntPtr(_picture->v), planeSizeInBytes);
    }

    int Picture::CalculateChromaPlaneSizeInBytes(int yPlaneSizeInPixels, KvazaarInterop::ChromaFormat chromaFormat) {
        int chromaPlaneSizeInPixels;
        switch (chromaFormat) {
        case KvazaarInterop::ChromaFormat::Csp400:
            // 4:0:0 - No chroma planes
            chromaPlaneSizeInPixels = 0;
            break;
        case KvazaarInterop::ChromaFormat::Csp420:
            // 4:2:0 - Chroma planes are 1/2 width and 1/2 height of Y plane
            chromaPlaneSizeInPixels = yPlaneSizeInPixels / 4;
            break;
        case KvazaarInterop::ChromaFormat::Csp422:
            // 4:2:2 - Chroma planes are 1/2 width and full height of Y plane
            chromaPlaneSizeInPixels = yPlaneSizeInPixels / 2;
            break;
        case KvazaarInterop::ChromaFormat::Csp444:
            // 4:4:4 - Chroma planes are same size as Y plane
            chromaPlaneSizeInPixels = yPlaneSizeInPixels;
            break;
        default:
            throw gcnew ArgumentException("Unknown chroma format");
        }
        // Convert from pixels to bytes
        return chromaPlaneSizeInPixels * sizeof(kvz_pixel);
    }

#pragma region IDisposable
    void Picture::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Picture::~Picture() {
        if (_disposed) {
            return;
        }

        this->!Picture();
        _disposed = true;
    }

    Picture::!Picture() {
        if (_picture) {
            auto api = Api::GetApi();
            api->picture_free(_picture);
            _picture = nullptr;
        }
    }
#pragma endregion
}