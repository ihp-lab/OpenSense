#include "pch.h"
#include "AccessUnitData.h"

using namespace System;

namespace HMInterop {
    AccessUnitData::AccessUnitData(uint8_t* data, int length, long long pts, int poc)
        : _data(data)
        , _length(length)
        , _pts(pts)
        , _poc(poc)
        , _disposed(false) {

        if (!data) {
            throw gcnew ArgumentNullException("data");
        }
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
        if (_data) {
            delete[] _data;
            _data = nullptr;
        }
    }
#pragma endregion
}
