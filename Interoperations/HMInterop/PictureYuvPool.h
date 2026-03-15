#pragma once

#include "PictureYuv.h"

using namespace System;
using namespace System::Collections::Concurrent;

namespace HMInterop {
    /// <summary>
    /// Thread-safe pool for PictureYuv objects, keyed by (width, height, chromaFormat).
    /// Reduces GC pressure from repeated allocation of large pixel buffers (~3MB per frame).
    /// </summary>
    public ref class PictureYuvPool abstract sealed {
    private:
        static ConcurrentDictionary<ValueTuple<int, int, int>, ConcurrentQueue<PictureYuv^>^>^ pools
            = gcnew ConcurrentDictionary<ValueTuple<int, int, int>, ConcurrentQueue<PictureYuv^>^>();

    public:
        /// <summary>
        /// Rent a PictureYuv with the specified dimensions. Reuses a pooled instance if available.
        /// </summary>
        static PictureYuv^ Rent(ChromaFormat chromaFormat, int width, int height);

        /// <summary>
        /// Return a PictureYuv to the pool for later reuse.
        /// </summary>
        static void Return(PictureYuv^ picYuv);
    };
}
