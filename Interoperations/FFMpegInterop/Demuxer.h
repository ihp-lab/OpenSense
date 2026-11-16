#pragma once

extern "C" {
#include <libavformat/avformat.h>
}

#include "CodecParameters.h"
#include "Packet.h"

using namespace System;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// Demuxer provides access to video and audio packets from a file using FFmpeg
    /// Reads the best video stream and the best audio stream
    /// </summary>
    public ref class Demuxer sealed : IDisposable {
    private:
        AVFormatContext* _formatContext;
        int _videoStreamIndex;
        int _audioStreamIndex;
        CodecParameters^ _videoParameters;
        CodecParameters^ _audioParameters;
        bool _readVideo;
        bool _readAudio;
        bool _started;
        bool _endOfFile;

        // All in .NET ticks. AV_NOPTS_VALUE until known.
        long long _presentationReference;
        long long _videoFirstDecodingTime;
        long long _videoLastDecodingOffset;
        long long _audioFirstDecodingTime;
        long long _audioLastDecodingOffset;

        void ApplySelection();
        static long long ToDecodingOffset(long long decodingTime, long long% first, long long% last);

    public:
        /// <summary>
        /// Initialize Demuxer with specified media file
        /// </summary>
        /// <param name="filename">Path to media file</param>
        Demuxer([NotNull] String^ filename);

        /// <summary>
        /// Gets the codec parameters of the video stream, null if the file has no video
        /// </summary>
        property CodecParameters^ VideoParameters {
            CodecParameters^ get();
        }

        /// <summary>
        /// Gets the codec parameters of the audio stream, null if the file has no audio
        /// </summary>
        property CodecParameters^ AudioParameters {
            CodecParameters^ get();
        }

        /// <summary>
        /// Selects which streams to read, both by default
        /// Must be called before reading the first packet
        /// </summary>
        /// <param name="readVideo">Whether to read the video stream</param>
        /// <param name="readAudio">Whether to read the audio stream</param>
        void SelectStreams(bool readVideo, bool readAudio);

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
