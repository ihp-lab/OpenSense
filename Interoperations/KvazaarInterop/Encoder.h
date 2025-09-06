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
    /// Managed wrapper for kvz_encoder
    /// </summary>
    public ref class Encoder : IDisposable {
    private:
        kvz_encoder* _encoder;
        bool _disposed;

    public:
        /// <summary>
        /// Creates encoder with specified configuration
        /// </summary>
        Encoder([NotNull] Config^ config);

        /// <summary>
        /// Get parameter sets (VPS, SPS, PPS)
        /// </summary>
        DataChunk^ GetHeaders();

        /// <summary>
        /// Get parameter sets (VPS, SPS, PPS) with length output
        /// </summary>
        DataChunk^ GetHeaders([Out] int% length);

        /// <summary>
        /// Encode a frame
        /// </summary>
        /// <param name="pictureIn">Input picture to encode, or nullptr to flush</param>
        /// <param name="pictureOut">Returns the reconstructed picture</param>
        /// <param name="sourceOut">Returns the original picture</param>
        /// <param name="infoOut">Returns frame information</param>
        /// <returns>Encoded data chunk, or nullptr if no output available</returns>
        DataChunk^ Encode(
            Picture^ pictureIn, 
            [Out] Picture^% pictureOut, 
            [Out] Picture^% sourceOut,
            [Out] FrameInfo^% infoOut
        );

        /// <summary>
        /// Encode a frame with length output
        /// </summary>
        DataChunk^ Encode(
            Picture^ pictureIn,
            [Out] int% length,
            [Out] Picture^% pictureOut,
            [Out] Picture^% sourceOut,
            [Out] FrameInfo^% infoOut
        );

        /// <summary>
        /// Simple encode that only returns the data chunk
        /// </summary>
        DataChunk^ Encode(Picture^ pictureIn);

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~Encoder();
        !Encoder();
#pragma endregion
    };
}