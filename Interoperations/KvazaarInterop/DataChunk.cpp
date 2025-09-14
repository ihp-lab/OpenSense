#include "pch.h"
#include "DataChunk.h"
#include "DataChunkEnumerator.h"

using namespace System;
using namespace System::Collections::Generic;

namespace KvazaarInterop {
    DataChunk::DataChunk(kvz_data_chunk* chunk)
        : _chunk(chunk)
        , _disposed(false) {

        if (!chunk) {
            throw gcnew ArgumentNullException("chunk");
        }
    }

#pragma region IEnumerable
    System::Collections::Generic::IEnumerator<ValueTuple<IntPtr, int>>^ DataChunk::GetEnumerator() {
        ThrowIfDisposed();
        return gcnew DataChunkEnumerator(this);
    }

    System::Collections::IEnumerator^ DataChunk::GetEnumerator2() {
        return GetEnumerator();
    }
#pragma endregion

#pragma region IDisposable

    void DataChunk::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    DataChunk::~DataChunk() {
        if (_disposed) {
            return;
        }

        this->!DataChunk();
        _disposed = true;
    }

    DataChunk::!DataChunk() {
        if (_chunk) {
            auto api = Api::GetApi();
            api->chunk_free(_chunk);
            _chunk = nullptr;
        }
    }

#pragma endregion
}