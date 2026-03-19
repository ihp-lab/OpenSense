#include "pch.h"
#include "Picture.h"

#include <cstring>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace KvazaarInterop {
    Picture::Picture(kvz_picture* picture)
        : _picture(picture)
        , _disposed(false)
        , _bitDepth(MaxBitDepth) {

        if (!picture) {
            throw gcnew ArgumentNullException("picture");
        }
    }

    Picture::Picture(KvazaarInterop::ChromaFormat chromaFormat, int width, int height, int bitDepth)
        : _picture(nullptr)
        , _disposed(false)
        , _bitDepth(bitDepth) {

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

    ValueTuple<IntPtr, int, int, int> Picture::GetPlaneAccess(ComponentId componentId) {
        ThrowIfDisposed();

        auto w = _picture->width;
        auto h = _picture->height;
        auto stride = _picture->stride;
        auto chromaFormat = static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);

        switch (componentId) {
        case ComponentId::Y:
            return ValueTuple<IntPtr, int, int, int>(IntPtr(_picture->y), w, h, stride);
        case ComponentId::U:
            if (!_picture->u) {
                throw gcnew InvalidOperationException("Picture has no U plane (Chroma400)");
            }
            break;
        case ComponentId::V:
            if (!_picture->v) {
                throw gcnew InvalidOperationException("Picture has no V plane (Chroma400)");
            }
            break;
        default:
            throw gcnew ArgumentOutOfRangeException("componentId");
        }

        // Chroma plane dimensions
        int chromaW, chromaH, chromaStride;
        switch (chromaFormat) {
        case KvazaarInterop::ChromaFormat::Csp420:
            chromaW = (w + 1) / 2;
            chromaH = (h + 1) / 2;
            chromaStride = (stride + 1) / 2;
            break;
        case KvazaarInterop::ChromaFormat::Csp422:
            chromaW = (w + 1) / 2;
            chromaH = h;
            chromaStride = (stride + 1) / 2;
            break;
        case KvazaarInterop::ChromaFormat::Csp444:
            chromaW = w;
            chromaH = h;
            chromaStride = stride;
            break;
        default:
            throw gcnew InvalidOperationException("Unexpected chroma format for chroma plane access");
        }

        auto ptr = (componentId == ComponentId::U) ? _picture->u : _picture->v;
        return ValueTuple<IntPtr, int, int, int>(IntPtr(ptr), chromaW, chromaH, chromaStride);
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