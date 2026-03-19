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
    /// Managed wrapper for kvz_picture.
    ///
    /// Memory management model:
    /// - kvz_picture has an internal atomic reference count (kvz_image_copy_ref increments, kvz_image_free decrements).
    ///   The pixel buffer is freed only when the refcount reaches zero.
    /// - encoder_encode() internally calls kvz_image_copy_ref on the input picture, so the caller may safely
    ///   release (dispose) its own reference after encode returns. The encoder releases its reference
    ///   after GOP buffering completes.
    /// - Pictures CANNOT be pooled. The native refcount is opaque to managed code (no API to query it),
    ///   so there is no safe way to determine whether the encoder still holds a reference.
    ///   Returning a Picture to a pool while the encoder retains a reference would allow new pixel data
    ///   to overwrite a frame that is still being encoded. Always create a new Picture per frame and let
    ///   Shared&lt;Picture&gt; / IDisposable handle lifetime.
    /// - Unlike HM's PictureYuv (which CAN be pooled because HM copies pixel data into its own internal
    ///   buffers and does not retain external references), Kvazaar's encoder directly references the
    ///   input picture's pixel buffer via copy_ref.
    /// - Pixel buffers are allocated by kvz_image_alloc (single SIMD-aligned malloc, Y+U+V contiguous).
    ///   No public API exists to supply externally-allocated memory.
    /// </summary>
    public ref class Picture : IDisposable {
    private:
        kvz_picture* _picture;
        int _bitDepth;

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
        /// Maximum bit depth for CopyYPlane/CopyUPlane/CopyVPlane input data.
        /// Determined at compile time from the native pixel type (kvz_pixel).
        /// </summary>
        literal int MaxBitDepth = sizeof(kvz_pixel) * 8;

        /// <summary>
        /// Creates a new picture with specified chroma format, dimensions, and data bit depth
        /// </summary>
        Picture(KvazaarInterop::ChromaFormat chromaFormat, int width, int height, int bitDepth);

        /// <summary>
        /// Gets or sets the actual data bit depth of the pixel values.
        /// This is a managed-only field (not part of the native kvz_picture struct).
        /// Kvazaar's native picture has no bit depth metadata, so this must be set
        /// by the producer (e.g. ImageToPictureConverter sets it to OutputBitDepth).
        /// Defaults to MaxBitDepth (compile-time KVZ_BIT_DEPTH).
        /// </summary>
        property int BitDepth {
            int get() {
                ThrowIfDisposed();
                return _bitDepth;
            }
            void set(int value) {
                ThrowIfDisposed();
                _bitDepth = value;
            }
        }

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
        /// Gets or sets the presentation timestamp.
        /// </summary>
        /// <remarks>
        /// Setter is used by FileWriter to set PTS before passing to encoder.
        /// Mutating a Picture that is shared via Psi's Shared&lt;T&gt; is not safe if multiple
        /// consumers receive the same instance. This is acceptable because in practice only one
        /// encoder consumes each Picture, but be aware of this if the pipeline topology changes.
        /// </remarks>
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
        /// Gets the decompression timestamp
        /// </summary>
        property long long DTS {
            long long get() {
                ThrowIfDisposed();
                return _picture->dts;
            }
        }

        /// <summary>
        /// Gets the interlacing mode
        /// </summary>
        property Interlacing Interlacing {
            KvazaarInterop::Interlacing get() {
                ThrowIfDisposed();
                return static_cast<KvazaarInterop::Interlacing>(_picture->interlacing);
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
        /// Get plane data pointer and dimensions for a given component.
        /// Stride is in pixels (not bytes). Multiply by sizeof(kvz_pixel) for byte stride.
        /// For Chroma400, requesting U or V throws InvalidOperationException.
        /// </summary>
        /// <param name="componentId">Which plane to access (Y, U, or V)</param>
        /// <returns>Tuple with (DataPointer, Width, Height, StrideInPixels)</returns>
        [returnvalue: TupleElementNames(gcnew array<String^>{"Data", "Width", "Height", "Stride"})]
        ValueTuple<IntPtr, int, int, int> GetPlaneAccess(ComponentId componentId);

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
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Picture();
        !Picture();
#pragma endregion
    };
}