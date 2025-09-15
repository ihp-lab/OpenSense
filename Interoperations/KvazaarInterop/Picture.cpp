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

        auto planeSize = _picture->stride * _picture->height;
        if (length > planeSize) {
            throw gcnew ArgumentException("Data length exceeds Y plane size");
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

        memcpy(_picture->v, data.ToPointer(), length);
    }

    ValueTuple<IntPtr, int> Picture::GetYPlane() {
        ThrowIfDisposed();

        auto planeSize = _picture->stride * _picture->height;
        return ValueTuple<IntPtr, int>(IntPtr(_picture->y), planeSize);
    }

    ValueTuple<IntPtr, int> Picture::GetUPlane() {
        ThrowIfDisposed();

        if (!_picture->u) {
            return ValueTuple<IntPtr, int>(IntPtr::Zero, 0);
        }

        auto chromaHeight = _picture->height;
        auto chromaStride = _picture->stride;

        if (_picture->chroma_format == KVZ_CSP_420) {
            chromaHeight /= 2;
            chromaStride /= 2;
        } else if (_picture->chroma_format == KVZ_CSP_422) {
            chromaStride /= 2;
        }

        auto planeSize = chromaStride * chromaHeight;
        return ValueTuple<IntPtr, int>(IntPtr(_picture->u), planeSize);
    }

    ValueTuple<IntPtr, int> Picture::GetVPlane() {
        ThrowIfDisposed();

        if (!_picture->v) {
            return ValueTuple<IntPtr, int>(IntPtr::Zero, 0);
        }

        auto chromaHeight = _picture->height;
        auto chromaStride = _picture->stride;

        if (_picture->chroma_format == KVZ_CSP_420) {
            chromaHeight /= 2;
            chromaStride /= 2;
        } else if (_picture->chroma_format == KVZ_CSP_422) {
            chromaStride /= 2;
        }

        auto planeSize = chromaStride * chromaHeight;
        return ValueTuple<IntPtr, int>(IntPtr(_picture->v), planeSize);
    }

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
}