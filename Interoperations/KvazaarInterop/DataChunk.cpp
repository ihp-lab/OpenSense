#include "pch.h"
#include "DataChunk.h"
#include "DataChunkEnumerator.h"

using namespace System;
using namespace System::Collections::Generic;

namespace KvazaarInterop {
    DataChunk::DataChunk(kvz_data_chunk* chunk, int totalLength)
        : _chunk(chunk)
        , _disposed(false)
        , _totalLength(totalLength)
        , _count(0) {

        if (!chunk) {
            throw gcnew ArgumentNullException("chunk");
        }
        if (totalLength < 0) {
            throw gcnew ArgumentOutOfRangeException("totalLength", "TotalLength must be non-negative");
        }

        // Enumerate chunks to calculate count and verify total length
        int calculatedLength = 0;
        auto current = chunk;
        while (current) {
            _count++;
            calculatedLength += current->len;
            current = current->next;
        }

        if (calculatedLength != totalLength) {
            throw gcnew InvalidOperationException(
                String::Format(
                    "Length mismatch: API reported {0} bytes, but enumeration found {1} bytes",
                    totalLength,
                    calculatedLength
                )
            );
        }

        if (totalLength == 0) {
            throw gcnew ArgumentException("TotalLength cannot be zero", "totalLength");
        }

        if (_count == 0) {
            throw gcnew InvalidOperationException("Chunk count cannot be zero");
        }
    }

#pragma region IReadOnlyCollection
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