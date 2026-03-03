#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComPicYuv.h"
#pragma managed(pop)

#include "PicYuv.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace HMInterop {
    PicYuv::PicYuv(TComPicYuv* picYuv, bool ownsMemory)
        : _picYuv(picYuv)
        , _disposed(false)
        , _ownsMemory(ownsMemory) {

        if (!picYuv) {
            throw gcnew ArgumentNullException("picYuv");
        }
    }

    PicYuv::PicYuv(HMInterop::ChromaFormat chromaFormat, int width, int height)
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

    int PicYuv::Width::get() {
        ThrowIfDisposed();
        return _picYuv->getWidth(COMPONENT_Y);
    }

    int PicYuv::Height::get() {
        ThrowIfDisposed();
        return _picYuv->getHeight(COMPONENT_Y);
    }

    int PicYuv::Stride::get() {
        ThrowIfDisposed();
        return _picYuv->getStride(COMPONENT_Y);
    }

    HMInterop::ChromaFormat PicYuv::ChromaFormat::get() {
        ThrowIfDisposed();
        return static_cast<HMInterop::ChromaFormat>(_picYuv->getChromaFormat());
    }

    TComPicYuv* PicYuv::InternalPicYuv::get() {
        ThrowIfDisposed();
        return _picYuv;
    }

    void PicYuv::CopyYPlane(IntPtr data, int lengthInBytes) {
        ThrowIfDisposed();
        CopyPlaneData(data, lengthInBytes, COMPONENT_Y);
    }

    void PicYuv::CopyUPlane(IntPtr data, int lengthInBytes) {
        ThrowIfDisposed();
        CopyPlaneData(data, lengthInBytes, COMPONENT_Cb);
    }

    void PicYuv::CopyVPlane(IntPtr data, int lengthInBytes) {
        ThrowIfDisposed();
        CopyPlaneData(data, lengthInBytes, COMPONENT_Cr);
    }

    void PicYuv::CopyPlaneData(IntPtr data, int lengthInBytes, int componentId) {
        if (data == IntPtr::Zero) {
            throw gcnew ArgumentNullException("data");
        }
        if (lengthInBytes < 0) {
            throw gcnew ArgumentOutOfRangeException("lengthInBytes", "Length must be non-negative");
        }

        auto compId = static_cast<ComponentID>(componentId);
        auto planeWidth = _picYuv->getWidth(compId);
        auto planeHeight = _picYuv->getHeight(compId);
        auto expectedBytes = planeWidth * planeHeight * static_cast<int>(sizeof(uint16_t));

        if (lengthInBytes > expectedBytes) {
            throw gcnew ArgumentException(System::String::Format("Data length ({0} bytes) exceeds plane size ({1} bytes). Plane dimensions: {2}x{3}", lengthInBytes, expectedBytes, planeWidth, planeHeight));
        }

        auto src = static_cast<uint16_t*>(data.ToPointer());
        auto dst = _picYuv->getAddr(compId);
        auto stride = _picYuv->getStride(compId);
        auto pixelCount = lengthInBytes / sizeof(uint16_t);
        auto srcWidth = static_cast<int>(pixelCount) / planeHeight;
        if (srcWidth > planeWidth) {
            srcWidth = planeWidth;
        }

        // Row-by-row copy with stride handling and widening (uint16 -> Pel/Int)
        for (auto y = 0; y < planeHeight; y++) {
            for (auto x = 0; x < srcWidth; x++) {
                dst[y * stride + x] = static_cast<Pel>(src[y * srcWidth + x]);
            }
        }
    }

    array<System::Byte>^ PicYuv::GetYPlaneAs16Bit() {
        ThrowIfDisposed();
        return GetPlaneAs16Bit(COMPONENT_Y);
    }

    array<System::Byte>^ PicYuv::GetUPlaneAs16Bit() {
        ThrowIfDisposed();
        return GetPlaneAs16Bit(COMPONENT_Cb);
    }

    array<System::Byte>^ PicYuv::GetVPlaneAs16Bit() {
        ThrowIfDisposed();
        return GetPlaneAs16Bit(COMPONENT_Cr);
    }

    array<System::Byte>^ PicYuv::GetPlaneAs16Bit(int componentId) {
        auto compId = static_cast<ComponentID>(componentId);
        auto planeWidth = _picYuv->getWidth(compId);
        auto planeHeight = _picYuv->getHeight(compId);
        auto pixelCount = planeWidth * planeHeight;
        auto bytesNeeded = pixelCount * static_cast<int>(sizeof(uint16_t));

        auto result = gcnew array<System::Byte>(bytesNeeded);
        pin_ptr<System::Byte> pinned = &result[0];
        auto dstPtr = reinterpret_cast<uint16_t*>(pinned);

        auto src = _picYuv->getAddr(compId);
        auto stride = _picYuv->getStride(compId);

        // Row-by-row copy with stride handling and narrowing (Pel/Int -> uint16)
        for (auto y = 0; y < planeHeight; y++) {
            for (auto x = 0; x < planeWidth; x++) {
                dstPtr[y * planeWidth + x] = static_cast<uint16_t>(src[y * stride + x]);
            }
        }

        return result;
    }

    System::ValueTuple<IntPtr, int> PicYuv::GetYPlane() {
        ThrowIfDisposed();

        auto stride = _picYuv->getStride(COMPONENT_Y);
        auto totalHeight = _picYuv->getTotalHeight(COMPONENT_Y);
        auto sizeInBytes = stride * totalHeight * static_cast<int>(sizeof(Pel));
        return System::ValueTuple<IntPtr, int>(IntPtr(_picYuv->getBuf(COMPONENT_Y)), sizeInBytes);
    }

#pragma region IDisposable
    void PicYuv::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
        }
    }

    PicYuv::~PicYuv() {
        if (_disposed) {
            return;
        }

        this->!PicYuv();
        _disposed = true;
    }

    PicYuv::!PicYuv() {
        if (_picYuv && _ownsMemory) {
            _picYuv->destroy();
            delete _picYuv;
        }
        _picYuv = nullptr;
    }
#pragma endregion
}
