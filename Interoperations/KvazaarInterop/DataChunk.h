#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Api.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_data_chunk
    /// </summary>
    public ref class DataChunk : IDisposable {
    private:
        kvz_data_chunk* _chunk;
        bool _disposed;

    internal:
        /// <summary>
        /// Creates a DataChunk from native pointer
        /// Takes ownership of the chunk
        /// </summary>
        DataChunk(kvz_data_chunk* chunk);

    public:
        /// <summary>
        /// Get all data as a managed byte array
        /// </summary>
        array<Byte>^ GetData();

        /// <summary>
        /// Gets the total length of data
        /// </summary>
        property int Length {
            int get();
        }

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~DataChunk();
        !DataChunk();
#pragma endregion
    };
}