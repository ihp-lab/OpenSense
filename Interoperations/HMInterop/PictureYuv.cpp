#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComPicYuv.h"
#pragma managed(pop)

#include "PictureYuv.h"

using namespace System;

namespace HMInterop {
    PictureYuv::PictureYuv(TComPicYuv* picYuv, bool ownsMemory)
        : _picYuv(picYuv)
        , _disposed(false)
        , _ownsMemory(ownsMemory) {

        if (!picYuv) {
            throw gcnew ArgumentNullException("picYuv");
        }
    }

    PictureYuv::PictureYuv(HMInterop::ChromaFormat chromaFormat, int width, int height)
        : _picYuv(nullptr)
        , _disposed(false)
        , _ownsMemory(true) {

        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }

        _picYuv = new TComPicYuv();
        _picYuv->createWithoutCUInfo(width, height, static_cast<::ChromaFormat>(chromaFormat), false);
    }

    int PictureYuv::Width::get() {
        ThrowIfDisposed();
        return _picYuv->getWidth(COMPONENT_Y);
    }

    int PictureYuv::Height::get() {
        ThrowIfDisposed();
        return _picYuv->getHeight(COMPONENT_Y);
    }

    int PictureYuv::Stride::get() {
        ThrowIfDisposed();
        return _picYuv->getStride(COMPONENT_Y);
    }

    HMInterop::ChromaFormat PictureYuv::ChromaFormat::get() {
        ThrowIfDisposed();
        return static_cast<HMInterop::ChromaFormat>(_picYuv->getChromaFormat());
    }

    TComPicYuv* PictureYuv::InternalPicYuv::get() {
        ThrowIfDisposed();
        return _picYuv;
    }

    ValueTuple<IntPtr, int, int, int> PictureYuv::GetPlaneAccess(HMInterop::ComponentId componentId) {
        ThrowIfDisposed();

        auto compId = static_cast<ComponentID>(componentId);
        auto data = _picYuv->getAddr(compId);
        auto width = _picYuv->getWidth(compId);
        auto height = _picYuv->getHeight(compId);
        auto stride = _picYuv->getStride(compId);

        return ValueTuple<IntPtr, int, int, int>(IntPtr(data), width, height, stride);
    }

#pragma region IDisposable
    void PictureYuv::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
        }
    }

    PictureYuv::~PictureYuv() {
        if (_disposed) {
            return;
        }

        this->!PictureYuv();
        _disposed = true;
    }

    PictureYuv::!PictureYuv() {
        if (_picYuv && _ownsMemory) {
            _picYuv->destroy();
            delete _picYuv;
        }
        _picYuv = nullptr;
    }
#pragma endregion
}
