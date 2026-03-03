// NOTE: This header uses cli::array<T> instead of array<T> because HM's TComList.h
// has "using namespace std;" which pulls std::array into the global namespace.
// When this header is included from .cpp files that also include HM headers (e.g.
// Encoder.cpp, Decoder.cpp), bare "array" becomes ambiguous with std::array.
#pragma once

#include <cstdint>
#include "Enums.h"

using namespace System;
using namespace System::Runtime::CompilerServices;

class TComPicYuv;

namespace HMInterop {
    /// <summary>
    /// Managed wrapper for TComPicYuv
    /// </summary>
    public ref class PicYuv : IDisposable {
    private:
        TComPicYuv* _picYuv;
        bool _ownsMemory;

    internal:
        /// <summary>
        /// Creates a PicYuv wrapper around an existing TComPicYuv.
        /// </summary>
        /// <param name="picYuv">Native TComPicYuv pointer</param>
        /// <param name="ownsMemory">If true, this wrapper owns the memory and will free it on dispose</param>
        PicYuv(TComPicYuv* picYuv, bool ownsMemory);

    public:
        /// <summary>
        /// Maximum bit depth for CopyYPlane/CopyUPlane/CopyVPlane input data.
        /// Determined at compile time from the input pixel type (uint16_t).
        /// </summary>
        literal int MaxBitDepth = sizeof(uint16_t) * 8;

        /// <summary>
        /// Creates a new picture with specified chroma format and dimensions.
        /// Uses createWithoutCUInfo (no CU offset tables).
        /// </summary>
        PicYuv(HMInterop::ChromaFormat chromaFormat, int width, int height);

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
        /// Copy 16-bit pixel data into the Y plane.
        /// Widens uint16 to Pel (Int) during copy.
        /// </summary>
        /// <param name="data">Pointer to uint16_t pixel data</param>
        /// <param name="lengthInBytes">Length of the data in bytes</param>
        void CopyYPlane(IntPtr data, int lengthInBytes);

        /// <summary>
        /// Copy 16-bit pixel data into the U plane.
        /// Widens uint16 to Pel (Int) during copy.
        /// </summary>
        void CopyUPlane(IntPtr data, int lengthInBytes);

        /// <summary>
        /// Copy 16-bit pixel data into the V plane.
        /// Widens uint16 to Pel (Int) during copy.
        /// </summary>
        void CopyVPlane(IntPtr data, int lengthInBytes);

        /// <summary>
        /// Get Y plane data as 16-bit, narrowing from Pel (Int) to uint16.
        /// Returns a byte array containing the 16-bit pixel data (no stride padding).
        /// </summary>
        cli::array<Byte>^ GetYPlaneAs16Bit();

        /// <summary>
        /// Get U plane data as 16-bit, narrowing from Pel (Int) to uint16.
        /// </summary>
        cli::array<Byte>^ GetUPlaneAs16Bit();

        /// <summary>
        /// Get V plane data as 16-bit, narrowing from Pel (Int) to uint16.
        /// </summary>
        cli::array<Byte>^ GetVPlaneAs16Bit();

        /// <summary>
        /// Get Y plane raw Pel pointer and total size in bytes (including stride padding).
        /// Caller must understand that Pel is Int (32-bit) when HIGH_BIT_DEPTH is on.
        /// </summary>
        [returnvalue: TupleElementNames(gcnew cli::array<String^>{"Data", "Length"})]
        ValueTuple<IntPtr, int> GetYPlane();

    internal:
        /// <summary>
        /// Gets the internal TComPicYuv pointer
        /// </summary>
        property TComPicYuv* InternalPicYuv {
            TComPicYuv* get();
        }

    private:
        /// <summary>
        /// Copy 16-bit data into a component plane, widening uint16 to Pel.
        /// </summary>
        void CopyPlaneData(IntPtr data, int lengthInBytes, int componentId);

        /// <summary>
        /// Get a component plane as 16-bit data, narrowing Pel to uint16.
        /// </summary>
        cli::array<Byte>^ GetPlaneAs16Bit(int componentId);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~PicYuv();
        !PicYuv();
#pragma endregion
    };
}
