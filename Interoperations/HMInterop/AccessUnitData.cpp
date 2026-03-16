#include "pch.h"
#include "AccessUnitData.h"

using namespace System;
using namespace System::Buffers;

namespace HMInterop {
    AccessUnitData::AccessUnitData(IMemoryOwner<Byte>^ owner, int length, long long pts, int poc)
        : _owner(owner)
        , _length(length)
        , _pts(pts)
        , _poc(poc)
        , _disposed(false) {

        ArgumentNullException::ThrowIfNull(owner, "owner");
        if (length <= 0) {
            throw gcnew ArgumentOutOfRangeException("length", "Length must be positive");
        }
    }

#pragma region IDisposable
    void AccessUnitData::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    AccessUnitData::~AccessUnitData() {
        if (_disposed) {
            return;
        }

        this->!AccessUnitData();
        _disposed = true;
    }

    AccessUnitData::!AccessUnitData() {
        if (_owner != nullptr) {
            delete _owner;
            _owner = nullptr;
        }
    }
#pragma endregion
}
