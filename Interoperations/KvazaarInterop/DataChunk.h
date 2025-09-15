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
    /// Managed wrapper for kvz_data_chunk that provides read-only collection access to data
    /// </summary>
    //[TupleElementNames(gcnew array<String^>{"Data", "Length"})] //Not working, and will have compile error when used
    public ref class DataChunk : System::Collections::Generic::IReadOnlyCollection<ValueTuple<IntPtr, int>>, IDisposable {
    private:
        kvz_data_chunk* _chunk;
        bool _disposed;
        int _totalLength;
        int _count;

    internal:
        /// <summary>
        /// Creates a DataChunk from native pointer
        /// Takes ownership of the chunk
        /// </summary>
        /// <param name="chunk">Native chunk pointer</param>
        /// <param name="totalLength">Total byte count of all chunks in the linked list</param>
        DataChunk(kvz_data_chunk* chunk, int totalLength);

    public:
        /// <summary>
        /// Gets the total count of valid data bytes across all chunks in the linked list
        /// This value comes from kvazaar's len_out parameter and represents the actual encoded data size
        /// </summary>
        property int TotalLength{
            int get() {
                ThrowIfDisposed();
                return _totalLength;
            }
        }

#pragma region IReadOnlyCollection
    public:
        /// <summary>
        /// Gets the number of chunks in the linked list
        /// </summary>
        virtual property int Count {
            int get() {
                ThrowIfDisposed();
                return _count;
            }
        }

        /// <summary>
        /// Gets an enumerator that iterates through the data chunks as pointer and length pairs
        /// </summary>
        //[returnvalue: TupleElementNames(gcnew array<String^>{"Data", "Length"})] //Not working, and will have compile error when used
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