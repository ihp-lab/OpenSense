#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Config.h"
#include "DataChunk.h"
#include "FrameInfo.h"
#include "Api.h"
#include "Picture.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_encoder.
    ///
    /// Thread safety: Kvazaar's kvz_strategyselector_init() modifies unprotected global state
    /// (kvz_g_hardware_flags, kvz_g_strategies_in_use, kvz_g_strategies_available) during
    /// encoder_open(). A process-wide lock serializes construction and destruction to prevent
    /// data races. Once created, encoder instances are fully independent and may encode
    /// concurrently without synchronization.
    /// </summary>
    public ref class Encoder : IDisposable {
    private:
        static Object^ s_lock = gcnew Object();
        kvz_encoder* _encoder;

    public:
        /// <summary>
        /// Creates encoder with specified configuration
        /// </summary>
        Encoder([NotNull] Config^ config);

        /// <summary>
        /// Get parameter sets (VPS, SPS, PPS)
        /// </summary>
        /// <returns>DataChunk containing header data</returns>
        [returnvalue: NotNull]
        DataChunk^ GetHeaders();

        /// <summary>
        /// Encode a frame
        /// </summary>
        /// <param name="inputPicture">Input picture to encode, or nullptr to flush</param>
        /// <param name="noDataChunk">Set to true to skip returning encoded data chunk</param>
        /// <param name="noFrameInfo">Set to true to skip returning frame info</param>
        /// <param name="noSourcePicture">Set to true to skip returning source picture</param>
        /// <param name="noReconstructedPicture">Set to true to skip returning reconstructed picture</param>
        /// <returns>ValueTuple containing (DataChunk, FrameInfo, SourcePicture, ReconstructedPicture)</returns>
        [returnvalue: TupleElementNames(gcnew array<String^>{"DataChunk", "FrameInfo", "SourcePicture", "ReconstructedPicture"})]
        ValueTuple<DataChunk^, FrameInfo^, Picture^, Picture^> Encode(
            Picture^ inputPicture,
            bool noDataChunk,
            bool noFrameInfo,
            bool noSourcePicture,
            bool noReconstructedPicture
        );

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Encoder();
        !Encoder();
#pragma endregion
    };
}