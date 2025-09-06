#include "pch.h"
#include "Picture.h"

#include <cstring>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace KvazaarInterop {
    Picture::Picture(int width, int height)
        : _picture(nullptr)
        , _disposed(false) {
        
        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }

        auto api = Api::GetApi();
        _picture = api->picture_alloc(width, height);
        
        if (!_picture) {
            throw gcnew OutOfMemoryException("Failed to allocate picture");
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

    void Picture::CopyYPlane(array<Byte>^ data, int offset, int length) {
        ThrowIfDisposed();
        
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }
        if (offset < 0) {
            throw gcnew ArgumentOutOfRangeException("offset", "Offset must be non-negative");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }
        if (offset + length > data->Length) {
            throw gcnew ArgumentException("Offset and length exceed array bounds");
        }

        auto planeSize = _picture->stride * _picture->height;
        if (length > planeSize) {
            throw gcnew ArgumentException("Data length exceeds Y plane size");
        }

        pin_ptr<Byte> pinnedData = &data[offset];
        memcpy(_picture->y, pinnedData, length);
    }

    void Picture::CopyUPlane(array<Byte>^ data, int offset, int length) {
        ThrowIfDisposed();
        
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }
        if (offset < 0) {
            throw gcnew ArgumentOutOfRangeException("offset", "Offset must be non-negative");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }
        if (offset + length > data->Length) {
            throw gcnew ArgumentException("Offset and length exceed array bounds");
        }

        if (!_picture->u) {
            throw gcnew InvalidOperationException("Picture has no U plane");
        }

        pin_ptr<Byte> pinnedData = &data[offset];
        memcpy(_picture->u, pinnedData, length);
    }

    void Picture::CopyVPlane(array<Byte>^ data, int offset, int length) {
        ThrowIfDisposed();
        
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }
        if (offset < 0) {
            throw gcnew ArgumentOutOfRangeException("offset", "Offset must be non-negative");
        }
        if (length < 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be non-negative");
        }
        if (offset + length > data->Length) {
            throw gcnew ArgumentException("Offset and length exceed array bounds");
        }

        if (!_picture->v) {
            throw gcnew InvalidOperationException("Picture has no V plane");
        }

        pin_ptr<Byte> pinnedData = &data[offset];
        memcpy(_picture->v, pinnedData, length);
    }

    array<Byte>^ Picture::GetYPlane() {
        ThrowIfDisposed();
        
        auto planeSize = _picture->stride * _picture->height;
        auto data = gcnew array<Byte>(planeSize);
        
        pin_ptr<Byte> pinnedData = &data[0];
        memcpy(pinnedData, _picture->y, planeSize);
        
        return data;
    }

    array<Byte>^ Picture::GetUPlane() {
        ThrowIfDisposed();
        
        if (!_picture->u) {
            return nullptr;
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
        auto data = gcnew array<Byte>(planeSize);
        
        pin_ptr<Byte> pinnedData = &data[0];
        memcpy(pinnedData, _picture->u, planeSize);
        
        return data;
    }

    array<Byte>^ Picture::GetVPlane() {
        ThrowIfDisposed();
        
        if (!_picture->v) {
            return nullptr;
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
        auto data = gcnew array<Byte>(planeSize);
        
        pin_ptr<Byte> pinnedData = &data[0];
        memcpy(pinnedData, _picture->v, planeSize);
        
        return data;
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