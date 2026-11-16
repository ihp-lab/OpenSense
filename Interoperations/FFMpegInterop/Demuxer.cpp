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
        , _videoStreams(gcnew List<CodecParameters^>())
        , _audioStreams(gcnew List<CodecParameters^>())
        , _started(false)
        , _endOfFile(false)
        , _streams(nullptr)
        , _selected(nullptr)
        , _presentationReference(AV_NOPTS_VALUE)
        , _firstDecodingTimes(nullptr)
        , _lastDecodingOffsets(nullptr)
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

        auto count = static_cast<int>(_formatContext->nb_streams);
        _streams = gcnew array<CodecParameters^>(count);
        _selected = gcnew array<bool>(count);
        _firstDecodingTimes = gcnew array<long long>(count);
        _lastDecodingOffsets = gcnew array<long long>(count);
        for (auto i = 0; i < count; i++) {
            _firstDecodingTimes[i] = AV_NOPTS_VALUE;
            _lastDecodingOffsets[i] = AV_NOPTS_VALUE;
            auto stream = _formatContext->streams[i];
            // Cover art is not video, a single picture without timestamps
            if (stream->disposition & AV_DISPOSITION_ATTACHED_PIC) {
                continue;
            }
            auto type = stream->codecpar->codec_type;
            if (type != AVMEDIA_TYPE_VIDEO && type != AVMEDIA_TYPE_AUDIO) {
                continue;
            }
            _streams[i] = gcnew CodecParameters(stream->codecpar);
            _selected[i] = true;
            (type == AVMEDIA_TYPE_VIDEO ? _videoStreams : _audioStreams)->Add(_streams[i]);
        }

        ApplySelection();
    }

    IReadOnlyList<CodecParameters^>^ Demuxer::VideoStreams::get() {
        ThrowIfDisposed();
        return _videoStreams;
    }

    IReadOnlyList<CodecParameters^>^ Demuxer::AudioStreams::get() {
        ThrowIfDisposed();
        return _audioStreams;
    }

    void Demuxer::SelectStreams([NotNull] IEnumerable<CodecParameters^>^ streams) {
        ThrowIfDisposed();
        if (streams == nullptr) {
            throw gcnew ArgumentNullException("streams");
        }
        if (_started) {
            throw gcnew InvalidOperationException("Streams must be selected before reading the first packet.");
        }
        auto selected = gcnew HashSet<CodecParameters^>(streams);
        for (auto i = 0; i < _streams->Length; i++) {
            _selected[i] = _streams[i] != nullptr && selected->Contains(_streams[i]);
        }
        ApplySelection();
    }

    void Demuxer::ApplySelection() {
        _presentationReference = AV_NOPTS_VALUE;
        for (auto i = 0; i < _streams->Length; i++) {
            auto stream = _formatContext->streams[i];
            // Skip unused streams
            stream->discard = _selected[i] ? AVDISCARD_DEFAULT : AVDISCARD_ALL;
            if (!_selected[i]) {
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

                auto index = packet->stream_index;
                if (index < 0 || index >= _streams->Length || !_selected[index]) {
                    av_packet_unref(packet);
                    continue;
                }

                auto timeBase = _formatContext->streams[index]->time_base;
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

                if (_firstDecodingTimes[index] == AV_NOPTS_VALUE) {
                    _firstDecodingTimes[index] = decodingTime;
                }
                auto decodingOffset = decodingTime - _firstDecodingTimes[index];
                // Keep strictly increasing, required by Psi
                if (_lastDecodingOffsets[index] != AV_NOPTS_VALUE && decodingOffset <= _lastDecodingOffsets[index]) {
                    decodingOffset = _lastDecodingOffsets[index] + 1;
                }
                _lastDecodingOffsets[index] = decodingOffset;

                packet->pts = presentationTime - _presentationReference;
                packet->dts = decodingOffset;
                packet->duration = av_rescale_q(packet->duration, timeBase, TicksTimeBase);
                packet->time_base = TicksTimeBase;

                auto managed = gcnew Packet(packet, _streams[index]);
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
