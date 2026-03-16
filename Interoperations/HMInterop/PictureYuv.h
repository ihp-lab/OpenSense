// NOTE: This header uses cli::array<T> instead of array<T> because HM's TComList.h
// has "using namespace std;" which pulls std::array into the global namespace.
// When this header is included from .cpp files that also include HM headers (e.g.
// Encoder.cpp, Decoder.cpp), bare "array" becomes ambiguous with std::array.
#pragma once

#include "Enums.h"

using namespace System;
using namespace System::Runtime::CompilerServices;

class TComPicYuv;

namespace HMInterop {
    /// <summary>
    /// Managed wrapper for TComPicYuv
    /// </summary>
    public ref class PictureYuv : IDisposable {
    private:
        TComPicYuv* _picYuv;
        bool _ownsMemory;

    internal:
        /// <summary>
        /// Creates a PictureYuv wrapper around an existing TComPicYuv.
        /// </summary>
        /// <param name="picYuv">Native TComPicYuv pointer</param>
        /// <param name="ownsMemory">If true, this wrapper owns the memory and will free it on dispose</param>
        PictureYuv(TComPicYuv* picYuv, bool ownsMemory);

    public:
        /// <summary>
        /// Creates a new picture with specified chroma format and dimensions.
        /// Uses createWithoutCUInfo (no CU offset tables).
        /// </summary>
        PictureYuv(HMInterop::ChromaFormat chromaFormat, int width, int height);

        /// <summary>
        /// Gets the width of the picture in pixels (luma plane)
        /// </summary>
        property int Width {
            int get();
        }

        /// <summary>
        /// Gets the height of the picture in pixels (luma plane)
        /// </summary>
        property int Height {
            int get();
        }

        /// <summary>
        /// Gets the stride of the Y plane in Pel units
        /// </summary>
        property int Stride {
            int get();
        }

        /// <summary>
        /// Gets the chroma format
        /// </summary>
        property HMInterop::ChromaFormat ChromaFormat {
            HMInterop::ChromaFormat get();
        }

        /// <summary>
        /// Get raw Pel buffer access for any component plane.
        /// Returns (Data, Width, Height, Stride) where Data is a Pel* pointer,
        /// Width/Height are the plane's pixel dimensions, and Stride is in Pel units.
        /// Pel is Int (32-bit) when RExt__HIGH_BIT_DEPTH_SUPPORT is enabled.
        /// </summary>
        [returnvalue: TupleElementNames(gcnew cli::array<String^>{"Data", "Width", "Height", "Stride"})]
        ValueTuple<IntPtr, int, int, int> GetPlaneAccess(ComponentId componentId);

    internal:
        /// <summary>
        /// Gets the internal TComPicYuv pointer
        /// </summary>
        property TComPicYuv* InternalPicYuv {
            TComPicYuv* get();
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~PictureYuv();
        !PictureYuv();
#pragma endregion
    };
}
