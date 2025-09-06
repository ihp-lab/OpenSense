#pragma once

// Include minimp4 declarations only (not implementation)
extern "C" {
#include <minimp4.h>
}

#include "Enums.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace Minimp4Interop {
    /// <summary>
    /// Simplified H.264/H.265 video writer for MP4 files
    /// </summary>
    public ref class H26xWriter : IDisposable {
    private:
        mp4_h26x_writer_t* _writer;
        FileStream^ _fileStream;
        GCHandle _gcHandle;
        bool _disposed;
        bool _isHevc;
        int _width;
        int _height;


    public:
        /// <summary>
        /// Creates a new H.264/H.265 video writer
        /// </summary>
        /// <param name="filename">Output MP4 file path</param>
        /// <param name="width">Video width in pixels</param>
        /// <param name="height">Video height in pixels</param>
        /// <param name="isHevc">True for HEVC/H.265, false for AVC/H.264</param>
        H26xWriter(String^ filename, int width, int height, bool isHevc);

        /// <summary>
        /// Creates a new H.264/H.265 video writer with frame rate
        /// </summary>
        /// <param name="filename">Output MP4 file path</param>
        /// <param name="width">Video width in pixels</param>
        /// <param name="height">Video height in pixels</param>
        /// <param name="frameRate">Frame rate (fps)</param>
        /// <param name="isHevc">True for HEVC/H.265, false for AVC/H.264</param>
        H26xWriter(String^ filename, int width, int height, double frameRate, bool isHevc);

#pragma region Methods

        /// <summary>
        /// Writes NAL unit data to the MP4 file
        /// </summary>
        /// <param name="nalData">NAL unit data (with or without start codes)</param>
        /// <param name="timestamp">Presentation timestamp in milliseconds</param>
        void WriteNal(array<Byte>^ nalData, long long timestamp);

        /// <summary>
        /// Writes NAL unit data to the MP4 file
        /// </summary>
        /// <param name="nalData">NAL unit data pointer</param>
        /// <param name="size">NAL data size in bytes</param>
        /// <param name="timestamp">Presentation timestamp in milliseconds</param>
        void WriteNal(IntPtr nalData, int size, long long timestamp);

        /// <summary>
        /// Closes the writer and finalizes the MP4 file
        /// </summary>
        void Close();

#pragma endregion

#pragma region Properties

        /// <summary>
        /// Gets whether this writer is for HEVC
        /// </summary>
        property bool IsHevc {
            bool get() {
                ThrowIfDisposed();
                return _isHevc;
            }
        }

        /// <summary>
        /// Gets the video width
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                return _width;
            }
        }

        /// <summary>
        /// Gets the video height
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
                return _height;
            }
        }

        /// <summary>
        /// Gets whether the writer is closed
        /// </summary>
        property bool IsClosed {
            bool get() {
                if (_writer == nullptr) {
                    return true;
                }
                return _writer->mux == nullptr;
            }
        }

#pragma endregion

    public:
        /// <summary>
        /// Internal callback handler
        /// </summary>
        int HandleWrite(int64_t offset, const void* buffer, size_t size);

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~H26xWriter();
        !H26xWriter();
#pragma endregion
    };
}