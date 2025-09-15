#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Enums.h"
#include "Api.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_picture
    /// </summary>
    public ref class Picture : IDisposable {
    private:
        kvz_picture* _picture;
        bool _disposed;

    internal:
        /// <summary>
        /// Creates a Picture wrapper around an existing kvz_picture
        /// Takes ownership of the native picture and will free it when disposed
        /// Note: Reference copying is not supported as kvz_image_copy_ref API is not exposed
        /// </summary>
        /// <param name="picture">Native kvz_picture pointer to take ownership of</param>
        Picture(kvz_picture* picture);

    public:
        /// <summary>
        /// Creates a new picture with specified chroma format and dimensions
        /// </summary>
        Picture(KvazaarInterop::ChromaFormat chromaFormat, int width, int height);

        /// <summary>
        /// Gets the width of the picture
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                return _picture->width;
            }
        }

        /// <summary>
        /// Gets the height of the picture
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
                return _picture->height;
            }
        }

        /// <summary>
        /// Gets the stride of the picture
        /// </summary>
        property int Stride {
            int get() {
                ThrowIfDisposed();
                return _picture->stride;
            }
        }

        /// <summary>
        /// Gets or sets the presentation timestamp
        /// </summary>
        property long long PTS {
            long long get() {
                ThrowIfDisposed();
                return _picture->pts;
            }
            void set(long long value) {
                ThrowIfDisposed();
                _picture->pts = value;
            }
        }

        /// <summary>
        /// Gets or sets the decompression timestamp
        /// </summary>
        property long long DTS {
            long long get() {
                ThrowIfDisposed();
                return _picture->dts;
            }
            void set(long long value) {
                ThrowIfDisposed();
                _picture->dts = value;
            }
        }

        /// <summary>
        /// Gets or sets the interlacing mode
        /// </summary>
        property Interlacing Interlacing {
            KvazaarInterop::Interlacing get() {
                ThrowIfDisposed();
                return static_cast<KvazaarInterop::Interlacing>(_picture->interlacing);
            }
            void set(KvazaarInterop::Interlacing value) {
                ThrowIfDisposed();
                _picture->interlacing = static_cast<kvz_interlacing>(value);
            }
        }

        /// <summary>
        /// Gets the chroma format
        /// </summary>
        property ChromaFormat ChromaFormat {
            KvazaarInterop::ChromaFormat get() {
                ThrowIfDisposed();
                return static_cast<KvazaarInterop::ChromaFormat>(_picture->chroma_format);
            }
        }

        /// <summary>
        /// Copy Y plane data from unmanaged memory
        /// </summary>
        void CopyYPlane(IntPtr data, int length);

        /// <summary>
        /// Copy U plane data from unmanaged memory
        /// </summary>
        void CopyUPlane(IntPtr data, int length);

        /// <summary>
        /// Copy V plane data from unmanaged memory
        /// </summary>
        void CopyVPlane(IntPtr data, int length);

        /// <summary>
        /// Get Y plane data pointer and length
        /// </summary>
        /// <returns>Tuple with Data pointer and Length in bytes</returns>
        [returnvalue: TupleElementNames(gcnew array<String^>{"Data", "Length"})]
        ValueTuple<IntPtr, int> GetYPlane();

        /// <summary>
        /// Get U plane data pointer and length
        /// </summary>
        /// <returns>Tuple with Data pointer and Length in bytes</returns>
        [returnvalue:TupleElementNames(gcnew array<String^>{"Data", "Length"})]
        ValueTuple<IntPtr, int> GetUPlane();

        /// <summary>
        /// Get V plane data pointer and length
        /// </summary>
        /// <returns>Tuple with Data pointer and Length in bytes</returns>
        [returnvalue:TupleElementNames(gcnew array<String^>{"Data", "Length"})]
        ValueTuple<IntPtr, int> GetVPlane();

    private:
        /// <summary>
        /// Calculate the size of a chroma plane based on the chroma format
        /// </summary>
        /// <param name="yPlaneSize">Size of the Y plane in bytes</param>
        /// <param name="chromaFormat">Chroma format</param>
        /// <returns>Size of U or V plane in bytes</returns>
        static int CalculateChromaPlaneSizeInBytes(int yPlaneSize, KvazaarInterop::ChromaFormat chromaFormat);

    internal:
        /// <summary>
        /// Gets the internal kvz_picture pointer
        /// </summary>
        property kvz_picture* InternalPicture {
            kvz_picture* get() {
                ThrowIfDisposed();
                return _picture;
            }
        }

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~Picture();
        !Picture();
#pragma endregion
    };
}