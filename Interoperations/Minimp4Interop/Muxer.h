#pragma once

extern "C" {
#include <minimp4.h>
}

#include "Enums.h"
#include "Track.h"

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace Minimp4Interop {
    /// <summary>
    /// Managed wrapper for MP4 muxer
    /// </summary>
    public ref class Muxer : IDisposable {
    private:
        MP4E_mux_t* _mux;
        FileStream^ _fileStream;
        GCHandle _gcHandle;
        bool _disposed;
        Dictionary<int, Track^>^ _tracks;
        MuxMode _mode;


    public:
        /// <summary>
        /// Creates a new MP4 muxer writing to a file
        /// </summary>
        Muxer(String^ filename);

        /// <summary>
        /// Creates a new MP4 muxer with specified mode
        /// </summary>
        Muxer(String^ filename, MuxMode mode);

#pragma region Methods

        /// <summary>
        /// Adds a track to the MP4 file
        /// </summary>
        /// <param name="track">Track configuration</param>
        /// <returns>Track ID for use with PutSample</returns>
        int AddTrack(Track^ track);

        /// <summary>
        /// Writes a sample to a track
        /// </summary>
        /// <param name="trackId">Track ID returned from AddTrack</param>
        /// <param name="data">Sample data</param>
        /// <param name="duration">Sample duration in track time scale units</param>
        /// <param name="sampleType">Whether this is a sync sample</param>
        void PutSample(int trackId, array<Byte>^ data, int duration, SampleType sampleType);

        /// <summary>
        /// Writes a sample to a track
        /// </summary>
        /// <param name="trackId">Track ID returned from AddTrack</param>
        /// <param name="data">Sample data pointer</param>
        /// <param name="size">Sample data size in bytes</param>
        /// <param name="duration">Sample duration in track time scale units</param>
        /// <param name="sampleType">Whether this is a sync sample</param>
        void PutSample(int trackId, IntPtr data, int size, int duration, SampleType sampleType);

        /// <summary>
        /// Closes the MP4 file and writes final metadata
        /// </summary>
        void Close();

#pragma endregion

#pragma region Properties

        /// <summary>
        /// Gets the muxing mode
        /// </summary>
        property MuxMode Mode {
            MuxMode get() {
                ThrowIfDisposed();
                return _mode;
            }
        }

        /// <summary>
        /// Gets whether the muxer is closed
        /// </summary>
        property bool IsClosed {
            bool get() {
                return _mux == nullptr;
            }
        }

#pragma endregion

#pragma region Internal

    public:
        /// <summary>
        /// Internal callback handler
        /// </summary>
        int HandleWrite(int64_t offset, const void* buffer, size_t size);

    internal:
        /// <summary>
        /// Gets the internal MP4E_mux_t pointer
        /// </summary>
        property MP4E_mux_t* InternalMux {
            MP4E_mux_t* get() {
                ThrowIfDisposed();
                return _mux;
            }
        }

#pragma endregion

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~Muxer();
        !Muxer();
#pragma endregion
    };
}