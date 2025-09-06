#include "pch.h"
#include "DataChunk.h"

using namespace System;

namespace KvazaarInterop {
    DataChunk::DataChunk(kvz_data_chunk* chunk)
        : _chunk(chunk)
        , _disposed(false) {
        
        if (!chunk) {
            throw gcnew ArgumentNullException("chunk");
        }
    }

    array<Byte>^ DataChunk::GetData() {
        ThrowIfDisposed();
        
        auto totalLength = 0;
        auto current = _chunk;
        
        while (current) {
            totalLength += current->len;
            current = current->next;
        }
        
        if (totalLength == 0) {
            return gcnew array<Byte>(0);
        }
        
        auto data = gcnew array<Byte>(totalLength);
        auto offset = 0;
        current = _chunk;
        
        while (current) {
            if (current->len > 0) {
                Marshal::Copy(IntPtr(current->data), data, offset, current->len);
                offset += current->len;
            }
            current = current->next;
        }
        
        return data;
    }

    int DataChunk::Length::get() {
        ThrowIfDisposed();
        
        auto totalLength = 0;
        auto current = _chunk;
        
        while (current) {
            totalLength += current->len;
            current = current->next;
        }
        
        return totalLength;
    }

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
}