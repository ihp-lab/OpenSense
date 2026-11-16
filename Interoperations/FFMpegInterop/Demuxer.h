#pragma once

extern "C" {
#include <libavformat/avformat.h>
}

#include "CodecParameters.h"
#include "Packet.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// Demuxer provides access to video and audio packets from a file using FFmpeg
    /// </summary>
    public ref class Demuxer sealed : IDisposable {
    private:
        AVFormatContext* _formatContext;
        List<CodecParameters^>^ _videoStreams;
        List<CodecParameters^>^ _audioStreams;
        bool _started;
        bool _endOfFile;

        // Indexed by the stream index of the file. Null for streams that are neither video nor audio.
        array<CodecParameters^>^ _streams;
        array<bool>^ _selected;

        // All in .NET ticks. AV_NOPTS_VALUE until known.
        long long _presentationReference;
        array<long long>^ _firstDecodingTimes;
        array<long long>^ _lastDecodingOffsets;

        void ApplySelection();

    public:
        /// <summary>
        /// Initialize Demuxer with specified media file
        /// </summary>
        /// <param name="filename">Path to media file</param>
        Demuxer([NotNull] String^ filename);

        /// <summary>
        /// Gets the codec parameters of the video streams, in the order of the file
        /// </summary>
        property IReadOnlyList<CodecParameters^>^ VideoStreams {
            IReadOnlyList<CodecParameters^>^ get();
        }

        /// <summary>
        /// Gets the codec parameters of the audio streams, in the order of the file
        /// </summary>
        property IReadOnlyList<CodecParameters^>^ AudioStreams {
            IReadOnlyList<CodecParameters^>^ get();
        }

        /// <summary>
        /// Selects which streams to read, all by default
        /// Must be called before reading the first packet
        /// </summary>
        /// <param name="streams">Streams to read, from VideoStreams and AudioStreams</param>
        void SelectStreams([NotNull] IEnumerable<CodecParameters^>^ streams);

        /// <summary>
        /// Reads the next packet
        /// </summary>
        /// <returns>The packet, or null if reached end of file</returns>
        Packet^ ReadPacket();

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Demuxer();
        !Demuxer();
#pragma endregion
    };
}
