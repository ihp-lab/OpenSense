#include "pch.h"
#include "Picture.h"
#include "PictureYuvPool.h"

namespace HMInterop {
    Picture::Picture(PictureYuv^ picYuv, int poc, SequenceParameterSet^ sps, PictureYuvOwnership ownership)
        : _picYuv(picYuv)
        , _poc(poc)
        , _sps(sps)
        , _ownership(ownership)
        , _disposed(false) {

        if (picYuv == nullptr) {
            throw gcnew System::ArgumentNullException("picYuv");
        }
    }

    PictureYuv^ Picture::PicYuv::get() {
        ThrowIfDisposed();
        return _picYuv;
    }

#pragma region IDisposable
    void Picture::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
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
        if (_picYuv == nullptr) {
            return;
        }

        switch (_ownership) {
        case PictureYuvOwnership::Owned:
            delete _picYuv;
            break;
        case PictureYuvOwnership::Pooled:
            PictureYuvPool::Return(_picYuv);
            break;
        }
        _picYuv = nullptr;
    }
#pragma endregion
}
