#include "pch.h"
#include "PictureSnapshot.h"
#include "PictureYuvPool.h"

namespace HMInterop {
    PictureSnapshot::PictureSnapshot(PictureYuv^ picYuv, int poc, SequenceParameterSetSnapshot^ sps, PictureYuvOwnership ownership)
        : _picYuv(picYuv)
        , _poc(poc)
        , _sps(sps)
        , _ownership(ownership)
        , _disposed(false) {

        if (picYuv == nullptr) {
            throw gcnew System::ArgumentNullException("picYuv");
        }
    }

    PictureYuv^ PictureSnapshot::PicYuv::get() {
        ThrowIfDisposed();
        return _picYuv;
    }

#pragma region IDisposable
    void PictureSnapshot::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew System::ObjectDisposedException(this->GetType()->FullName);
        }
    }

    PictureSnapshot::~PictureSnapshot() {
        if (_disposed) {
            return;
        }

        this->!PictureSnapshot();
        _disposed = true;
    }

    PictureSnapshot::!PictureSnapshot() {
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
