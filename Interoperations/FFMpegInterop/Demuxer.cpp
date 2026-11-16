#include "pch.h"

#include <string>
#include <cstring>

#include "Demuxer.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    namespace {
        const AVRational TicksTimeBase = { 1, 10000000 };
    }

    Demuxer::Demuxer([NotNull] String^ filename)
        : _formatContext(nullptr)
        , _videoStreamIndex(-1)
        , _audioStreamIndex(-1)
        , _videoParameters(nullptr)
        , _audioParameters(nullptr)
        , _readVideo(true)
        , _readAudio(true)
        , _started(false)
        , _endOfFile(false)
        , _presentationReference(AV_NOPTS_VALUE)
        , _videoFirstDecodingTime(AV_NOPTS_VALUE)
        , _videoLastDecodingOffset(AV_NOPTS_VALUE)
        , _audioFirstDecodingTime(AV_NOPTS_VALUE)
        , _audioLastDecodingOffset(AV_NOPTS_VALUE)
        , _disposed(false) {
        if (filename == nullptr) {
            throw gcnew ArgumentNullException("filename");
        }

        // FFmpeg expects UTF-8, default marshaling uses the system code page
        auto bytes = Text::Encoding::UTF8->GetBytes(filename);
        std::string utf8(bytes->Length, '\0');
        if (bytes->Length > 0) {
            pin_ptr<Byte> pinned = &bytes[0];
            std::memcpy(&utf8[0], pinned, bytes->Length);
        }

        AVFormatContext* context = nullptr;
        auto result = avformat_open_input(&context, utf8.c_str(), nullptr, nullptr);
        if (result < 0) {
            throw gcnew FileOpenException(filename, FFMpegException::DescribeError(result));
        }
        _formatContext = context;

        result = avformat_find_stream_info(_formatContext, nullptr);
        if (result < 0) {
            throw gcnew FFMpegException("Could not find stream information: " + FFMpegException::DescribeError(result));
        }

        auto video = av_find_best_stream(_formatContext, AVMEDIA_TYPE_VIDEO, -1, -1, nullptr, 0);
        if (video >= 0) {
            _videoStreamIndex = video;
            _videoParameters = gcnew CodecParameters(_formatContext->streams[video]->codecpar);
        }
        auto audio = av_find_best_stream(_formatContext, AVMEDIA_TYPE_AUDIO, -1, _videoStreamIndex, nullptr, 0);
        if (audio >= 0) {
            _audioStreamIndex = audio;
            _audioParameters = gcnew CodecParameters(_formatContext->streams[audio]->codecpar);
        }

        ApplySelection();
    }

    CodecParameters^ Demuxer::VideoParameters::get() {
        ThrowIfDisposed();
        return _videoParameters;
    }

    CodecParameters^ Demuxer::AudioParameters::get() {
        ThrowIfDisposed();
        return _audioParameters;
    }

    void Demuxer::SelectStreams(bool readVideo, bool readAudio) {
        ThrowIfDisposed();
        if (_started) {
            throw gcnew InvalidOperationException("Streams must be selected before reading the first packet.");
        }
        _readVideo = readVideo;
        _readAudio = readAudio;
        ApplySelection();
    }

    void Demuxer::ApplySelection() {
        _presentationReference = AV_NOPTS_VALUE;
        for (auto i = 0u; i < _formatContext->nb_streams; i++) {
            auto stream = _formatContext->streams[i];
            auto index = static_cast<int>(i);
            auto selected = (_readVideo && index == _videoStreamIndex) || (_readAudio && index == _audioStreamIndex);
            // Skip unused streams
            stream->discard = selected ? AVDISCARD_DEFAULT : AVDISCARD_ALL;
            if (!selected) {
                continue;
            }
            if (stream->start_time != AV_NOPTS_VALUE) {
                auto start = av_rescale_q(stream->start_time, stream->time_base, TicksTimeBase);
                if (_presentationReference == AV_NOPTS_VALUE || start < _presentationReference) {
                    _presentationReference = start;
                }
            }
        }
    }

    long long Demuxer::ToDecodingOffset(long long decodingTime, long long% first, long long% last) {
        if (first == AV_NOPTS_VALUE) {
            first = decodingTime;
        }
        auto offset = decodingTime - first;
        // Keep strictly increasing, required by Psi
        if (last != AV_NOPTS_VALUE && offset <= last) {
            offset = last + 1;
        }
        last = offset;
        return offset;
    }

    Packet^ Demuxer::ReadPacket() {
        ThrowIfDisposed();
        _started = true;
        if (_endOfFile) {
            return nullptr;
        }

        auto packet = av_packet_alloc();
        if (!packet) {
            throw gcnew OutOfMemoryException("Could not allocate a packet.");
        }
        try {
            while (true) {
                auto result = av_read_frame(_formatContext, packet);
                if (result == AVERROR_EOF) {
                    _endOfFile = true;
                    return nullptr;
                }
                if (result < 0) {
                    throw gcnew FFMpegException("Could not read a packet: " + FFMpegException::DescribeError(result));
                }

                auto isVideo = _readVideo && packet->stream_index == _videoStreamIndex;
                auto isAudio = _readAudio && packet->stream_index == _audioStreamIndex;
                if (!isVideo && !isAudio) {
                    av_packet_unref(packet);
                    continue;
                }

                auto timeBase = _formatContext->streams[packet->stream_index]->time_base;
                auto decodingTime = packet->dts != AV_NOPTS_VALUE ? packet->dts : packet->pts;
                auto presentationTime = packet->pts != AV_NOPTS_VALUE ? packet->pts : packet->dts;
                if (decodingTime == AV_NOPTS_VALUE) {
                    throw gcnew FFMpegException("The file has packets without timestamps, which is not supported.");
                }
                decodingTime = av_rescale_q(decodingTime, timeBase, TicksTimeBase);
                presentationTime = av_rescale_q(presentationTime, timeBase, TicksTimeBase);
                if (_presentationReference == AV_NOPTS_VALUE) {
                    // Start time unknown, use the first packet
                    _presentationReference = presentationTime;
                }

                packet->pts = presentationTime - _presentationReference;
                packet->dts = isVideo
                    ? ToDecodingOffset(decodingTime, _videoFirstDecodingTime, _videoLastDecodingOffset)
                    : ToDecodingOffset(decodingTime, _audioFirstDecodingTime, _audioLastDecodingOffset);
                packet->duration = av_rescale_q(packet->duration, timeBase, TicksTimeBase);
                packet->time_base = TicksTimeBase;

                auto managed = gcnew Packet(packet, isVideo ? _videoParameters : _audioParameters);
                packet = nullptr;
                return managed;
            }
        } finally {
            if (packet) {
                av_packet_free(&packet);
            }
        }
    }

#pragma region IDisposable
    void Demuxer::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("Demuxer");
        }
    }

    Demuxer::~Demuxer() {
        this->!Demuxer();
        GC::SuppressFinalize(this);
    }

    Demuxer::!Demuxer() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        // Codec parameters are not disposed here, packets in use may still reference them
        if (_formatContext) {
            auto context = _formatContext;
            avformat_close_input(&context);
            _formatContext = nullptr;
        }
    }
#pragma endregion
}
