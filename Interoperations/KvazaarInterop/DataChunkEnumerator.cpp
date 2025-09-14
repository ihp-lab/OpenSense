#include "pch.h"
#include "DataChunkEnumerator.h"
#include "DataChunk.h"

using namespace System;

namespace KvazaarInterop {

    DataChunkEnumerator::DataChunkEnumerator(DataChunk^ parent)
        : _parent(parent)
        , _current(nullptr) {

        if (parent == nullptr) {
            throw gcnew ArgumentNullException("parent");
        }
    }

#pragma region IEnumerator
    bool DataChunkEnumerator::MoveNext() {
        // Check if parent has been disposed
        _parent->ThrowIfDisposed();

        if (_current == nullptr) {
            // First call, get the first chunk from parent
            _current = _parent->InternalChunk;
        } else {
            // Move to next chunk
            _current = _current->next;
        }

        return _current != nullptr;
    }

    void DataChunkEnumerator::Reset() {
        // Check if parent has been disposed
        _parent->ThrowIfDisposed();

        _current = nullptr;
    }

    ValueTuple<IntPtr, int> DataChunkEnumerator::Current::get() {
        // Check if parent has been disposed
        _parent->ThrowIfDisposed();

        if (_current == nullptr) {
            throw gcnew InvalidOperationException("Enumeration has not started or has finished. Call MoveNext.");
        }

        // Return pointer and length directly for zero-copy access
        return ValueTuple<IntPtr, int>(IntPtr(_current->data), _current->len);
    }

    Object^ DataChunkEnumerator::Current2::get() {
        return Current;
    }
#pragma endregion
}