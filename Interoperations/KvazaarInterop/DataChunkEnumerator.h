#pragma once

extern "C" {
#include <kvazaar.h>
}

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::CompilerServices;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;

namespace KvazaarInterop {
    // Forward declaration
    ref class DataChunk;

    /// <summary>
    /// Enumerator for iterating over data chunks as pointer and length pairs
    /// </summary>
    [TupleElementNames(gcnew array<String^>{"Data", "Length"})]
    private value struct DataChunkEnumerator : System::Collections::Generic::IEnumerator<ValueTuple<IntPtr, int>> {
    private:
        DataChunk^ _parent;
        kvz_data_chunk* _current;

    internal:
        /// <summary>
        /// Creates an enumerator for the data chunks
        /// </summary>
        DataChunkEnumerator([NotNull] DataChunk^ parent);

#pragma region IEnumerator
    public:
        /// <summary>
        /// Advances the enumerator to the next element
        /// </summary>
        virtual bool MoveNext();

        /// <summary>
        /// Sets the enumerator to its initial position
        /// </summary>
        virtual void Reset();

        /// <summary>
        /// Gets the current element in the collection as pointer and length
        /// </summary>
        [TupleElementNames(gcnew array<String^>{"Data", "Length"})]
        property ValueTuple<IntPtr, int> Current {
            virtual ValueTuple<IntPtr, int> get();
        }

        /// <summary>
        /// Gets the current element as Object (non-generic IEnumerator)
        /// </summary>
        property Object^ Current2 {
            virtual Object^ get() sealed = System::Collections::IEnumerator::Current::get;
        }
    };
#pragma endregion
}