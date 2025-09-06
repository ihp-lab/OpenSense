#include "pch.h"
#include "FrameInfo.h"

#include <cstring>

using namespace System;

namespace KvazaarInterop {
    FrameInfo::FrameInfo(kvz_frame_info* info)
        : _info(info)
        , _disposed(false) {
        
        if (!info) {
            throw gcnew ArgumentNullException("info");
        }
    }

    FrameInfo::FrameInfo(const kvz_frame_info& info)
        : _info(nullptr)
        , _disposed(false) {
        
        _info = new kvz_frame_info();
        if (!_info) {
            throw gcnew OutOfMemoryException("Failed to allocate frame info");
        }
        
        memcpy(_info, &info, sizeof(kvz_frame_info));
    }

    array<int>^ FrameInfo::GetReferenceList(int listIndex) {
        ThrowIfDisposed();
        
        if (listIndex < 0 || listIndex > 1) {
            throw gcnew ArgumentOutOfRangeException("listIndex", "List index must be 0 or 1");
        }
        
        auto length = _info->ref_list_len[listIndex];
        if (length <= 0) {
            return gcnew array<int>(0);
        }
        
        auto list = gcnew array<int>(length);
        for (auto i = 0; i < length; i++) {
            list[i] = _info->ref_list[listIndex][i];
        }
        
        return list;
    }

    int FrameInfo::GetReferenceListLength(int listIndex) {
        ThrowIfDisposed();
        
        if (listIndex < 0 || listIndex > 1) {
            throw gcnew ArgumentOutOfRangeException("listIndex", "List index must be 0 or 1");
        }
        
        return _info->ref_list_len[listIndex];
    }

    void FrameInfo::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    FrameInfo::~FrameInfo() {
        if (_disposed) {
            return;
        }

        this->!FrameInfo();
        _disposed = true;
    }

    FrameInfo::!FrameInfo() {
        if (_info) {
            delete _info;
            _info = nullptr;
        }
    }
}