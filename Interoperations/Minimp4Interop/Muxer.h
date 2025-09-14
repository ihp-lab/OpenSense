#pragma once

extern "C" {
#include <minimp4.h>
}

#include "Enums.h"
#include "Track.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace Minimp4Interop {
    /// <summary>
    /// Managed wrapper for MP4 muxer
    /// This class manages MP4 container operations and track management.
    /// Note: The caller retains ownership of the Stream instance.
    /// </summary>
    public ref class Muxer : IDisposable {
    private:
        MP4E_mux_t* _mux;
        Stream^ _stream;
        GCHandle _gcHandle;
        MuxMode _mode;
        bool _disposed;


    public:
        /// <summary>
        /// Creates a new MP4 muxer with specified stream and mode
        /// Note: The caller retains ownership of the Stream. Muxer only uses it for writing.
        /// </summary>
        /// <param name="stream">Writable stream for MP4 output (caller retains ownership)</param>
        /// <param name="mode">Muxing mode (affects seeking requirements)</param>
        Muxer([NotNull] Stream^ stream, MuxMode mode);

#pragma region Methods

        /// <summary>
        /// Adds a track to the MP4 file
        /// Note: Track configuration is copied internally. The caller retains ownership of the Track object.
        /// </summary>
        /// <param name="track">Track configuration</param>
        /// <returns>Track ID for use with PutSample</returns>
        int AddTrack([NotNull] Track^ track);

        /// <summary>
        /// Writes a sample to a track
        /// </summary>
        /// <param name="trackId">Track ID returned from AddTrack</param>
        /// <param name="data">Sample data pointer</param>
        /// <param name="size">Sample data size in bytes</param>
        /// <param name="duration">Sample duration in track time scale units</param>
        /// <param name="sampleType">Whether this is a sync sample</param>
        void PutSample(int trackId, IntPtr data, int size, int duration, SampleType sampleType);

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

    internal:
        /// <summary>
        /// Internal callback handler
        /// </summary>
        int HandleWrite(int64_t offset, const void* buffer, size_t size);

#pragma region IDisposable
    private:
        /// <summary>
        /// Throws ObjectDisposedException if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();

    public:
        ~Muxer();
        !Muxer();
#pragma endregion
    };
}