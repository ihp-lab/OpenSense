#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Api.h"
#include "DataChunkEnumerator.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_data_chunk that provides enumerable access to data
    /// </summary>
    public ref class DataChunk : System::Collections::Generic::IEnumerable<ValueTuple<IntPtr, int>>, IDisposable {
    private:
        kvz_data_chunk* _chunk;
        bool _disposed;

    internal:
        /// <summary>
        /// Creates a DataChunk from native pointer
        /// Takes ownership of the chunk
        /// </summary>
        DataChunk(kvz_data_chunk* chunk);

#pragma region IEnumerable
    public:
        /// <summary>
        /// Gets an enumerator that iterates through the data chunks as pointer and length pairs
        /// </summary>
        [returnvalue:TupleElementNames(gcnew array<String^>{"Data", "Length"})]
        virtual System::Collections::Generic::IEnumerator<ValueTuple<IntPtr, int>>^ GetEnumerator();

        /// <summary>
        /// Gets a non-generic enumerator that iterates through the data chunks
        /// </summary>
        virtual System::Collections::IEnumerator^ GetEnumerator2() sealed =
            System::Collections::IEnumerable::GetEnumerator;

    internal:
        /// <summary>
        /// Gets the internal chunk pointer for use by enumerator
        /// </summary>
        property kvz_data_chunk* InternalChunk {
            kvz_data_chunk* get() { 
                ThrowIfDisposed();
                return _chunk; 
            }
        }
#pragma endregion

#pragma region IDisposable
    internal:
        /// <summary>
        /// Throws if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();
    public:
        ~DataChunk();
        !DataChunk();
#pragma endregion
    };
}