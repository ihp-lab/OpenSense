#pragma once

extern "C" {
#include <minimp4.h>
}

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;

namespace Minimp4Interop {
    /// <summary>
    /// Managed wrapper for MP4 demuxer.
    /// Wraps minimp4's MP4D_* API for reading MP4 files.
    /// Note: The caller retains ownership of the Stream instance.
    /// </summary>
    public ref class Demuxer : IDisposable {
    private:
        MP4D_demux_t* _demux;
        Stream^ _stream;
        GCHandle _gcHandle;

    public:
        /// <summary>
        /// Opens an MP4 file for demuxing.
        /// Note: The caller retains ownership of the Stream instance.
        /// </summary>
        /// <param name="stream">Readable and seekable stream for MP4 input (caller retains ownership)</param>
        Demuxer([NotNull] Stream^ stream);

#pragma region Properties

        /// <summary>
        /// Gets the number of tracks in the MP4 file
        /// </summary>
        property unsigned int TrackCount {
            unsigned int get() {
                ThrowIfDisposed();
                return _demux->track_count;
            }
        }

#pragma endregion

#pragma region Methods

        /// <summary>
        /// Gets the number of samples (frames) in a track
        /// </summary>
        unsigned int GetSampleCount(unsigned int trackIndex);

        /// <summary>
        /// Gets the timescale of a track (ticks per second, typically 90000 for video)
        /// </summary>
        unsigned int GetTimescale(unsigned int trackIndex);

        /// <summary>
        /// Gets the object type of a track (0x23 = HEVC, 0x21 = AVC)
        /// </summary>
        unsigned int GetObjectType(unsigned int trackIndex);

        /// <summary>
        /// Reads a sample (frame) from the specified track.
        /// Returns the raw sample data (length-prefixed NAL units for video).
        /// </summary>
        /// <param name="trackIndex">Track index</param>
        /// <param name="sampleIndex">Sample index (0-based)</param>
        /// <param name="timestamp">OUT: presentation timestamp in track timescale units</param>
        /// <param name="duration">OUT: sample duration in track timescale units</param>
        cli::array<Byte>^ ReadSample(
            unsigned int trackIndex,
            unsigned int sampleIndex,
            [Out] unsigned int% timestamp,
            [Out] unsigned int% duration
        );

        /// <summary>
        /// Reads a NAL unit from the HEVC decoder-specific info (HEVCDecoderConfigurationRecord).
        /// Iterates through all NAL arrays (VPS, SPS, PPS) in order.
        /// Returns null if index is out of range.
        /// </summary>
        /// <param name="trackIndex">Track index</param>
        /// <param name="index">NAL unit index (0-based, across all arrays)</param>
        cli::array<Byte>^ ReadParameterSet(unsigned int trackIndex, int index);

#pragma endregion

    internal:
        /// <summary>
        /// Internal callback handler for stream reading
        /// </summary>
        int HandleRead(int64_t offset, void* buffer, size_t size);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();
        void ThrowIfInvalidTrack(unsigned int trackIndex);

    public:
        ~Demuxer();
        !Demuxer();
#pragma endregion
    };
}
