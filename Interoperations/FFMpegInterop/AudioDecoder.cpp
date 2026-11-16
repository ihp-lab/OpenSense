#include "pch.h"

#include <cstring>

extern "C" {
#include <libavutil/mathematics.h>
}

#include "AudioDecoder.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    namespace {
        const AVRational TicksTimeBase = { 1, 10000000 };
    }

    AudioDecoder::AudioDecoder()
        : _codecContext(nullptr)
        , _frame(nullptr)
        , _flushed(false)
        , _nextPresentationTime(AV_NOPTS_VALUE)
        , _disposed(false) {
    }

    void AudioDecoder::Open(CodecParameters^ parameters) {
        if (parameters->MediaType != MediaType::Audio) {
            throw gcnew ArgumentException("The packet does not belong to an audio stream.", "packet");
        }
        auto native = parameters->InternalParameters;
        auto codec = avcodec_find_decoder(native->codec_id);
        if (!codec) {
            throw gcnew CodecException(String::Format("No decoder is available for codec {0}.", parameters->CodecName));
        }
        _codecContext = avcodec_alloc_context3(codec);
        if (!_codecContext) {
            throw gcnew OutOfMemoryException("Could not allocate a codec context.");
        }
        auto result = avcodec_parameters_to_context(_codecContext, native);
        if (result < 0) {
            throw gcnew CodecException("Could not apply codec parameters: " + FFMpegException::DescribeError(result));
        }
        // Packet timestamps are in .NET Ticks
        _codecContext->pkt_timebase = TicksTimeBase;
        result = avcodec_open2(_codecContext, codec, nullptr);
        if (result < 0) {
            throw gcnew CodecException("Could not open the decoder: " + FFMpegException::DescribeError(result));
        }
        _frame = av_frame_alloc();
        if (!_frame) {
            throw gcnew OutOfMemoryException("Could not allocate a frame.");
        }
    }

    void AudioDecoder::ReceiveSamples(IList<AudioSamples^>^ output) {
        while (true) {
            auto result = avcodec_receive_frame(_codecContext, _frame);
            if (result == AVERROR(EAGAIN) || result == AVERROR_EOF) {
                return;
            }
            if (result < 0) {
                throw gcnew CodecException("Could not decode a frame: " + FFMpegException::DescribeError(result));
            }
            try {
                auto nativeFormat = static_cast<AVSampleFormat>(_frame->format);
                auto format = SampleFormat::None;
                switch (av_get_packed_sample_fmt(nativeFormat)) {
                case AV_SAMPLE_FMT_U8:
                    format = SampleFormat::U8;
                    break;
                case AV_SAMPLE_FMT_S16:
                    format = SampleFormat::S16;
                    break;
                case AV_SAMPLE_FMT_S32:
                    format = SampleFormat::S32;
                    break;
                case AV_SAMPLE_FMT_FLT:
                    format = SampleFormat::FLT;
                    break;
                case AV_SAMPLE_FMT_DBL:
                    format = SampleFormat::DBL;
                    break;
                default:
                    throw gcnew CodecException(String::Format("Sample format {0} is not supported.", gcnew String(av_get_sample_fmt_name(nativeFormat))));
                }

                auto presentationTime = _frame->pts != AV_NOPTS_VALUE ? _frame->pts : _frame->best_effort_timestamp;
                if (presentationTime == AV_NOPTS_VALUE) {
                    // Continue from the previous frame
                    presentationTime = _nextPresentationTime;
                }
                if (presentationTime == AV_NOPTS_VALUE) {
                    throw gcnew CodecException("The decoder returned a frame without a timestamp.");
                }

                auto sampleCount = _frame->nb_samples;
                auto channelCount = _frame->ch_layout.nb_channels;
                auto sampleRate = _frame->sample_rate;
                if (sampleCount <= 0 || channelCount <= 0 || sampleRate <= 0) {
                    continue;
                }
                _nextPresentationTime = presentationTime + av_rescale(sampleCount, TicksTimeBase.den, sampleRate);

                auto sampleSize = av_get_bytes_per_sample(nativeFormat);
                auto data = gcnew array<Byte>(sampleCount * channelCount * sampleSize);
                pin_ptr<Byte> pinned = &data[0];
                auto target = static_cast<uint8_t*>(pinned);
                if (av_sample_fmt_is_planar(nativeFormat)) {
                    for (auto channel = 0; channel < channelCount; channel++) {
                        auto source = _frame->extended_data[channel];
                        for (auto i = 0; i < sampleCount; i++) {
                            std::memcpy(target + (static_cast<size_t>(i) * channelCount + channel) * sampleSize, source + static_cast<size_t>(i) * sampleSize, sampleSize);
                        }
                    }
                } else {
                    std::memcpy(target, _frame->extended_data[0], data->Length);
                }
                output->Add(gcnew AudioSamples(data, format, sampleRate, channelCount, sampleCount, presentationTime));
            } finally {
                av_frame_unref(_frame);
            }
        }
    }

    void AudioDecoder::Decode([NotNull] Packet^ packet, [NotNull] IList<AudioSamples^>^ output) {
        ThrowIfDisposed();
        if (packet == nullptr) {
            throw gcnew ArgumentNullException("packet");
        }
        if (output == nullptr) {
            throw gcnew ArgumentNullException("output");
        }
        if (_flushed) {
            throw gcnew InvalidOperationException("The decoder was flushed and cannot decode more packets.");
        }
        if (!_codecContext) {
            Open(packet->Parameters);
        }

        auto result = avcodec_send_packet(_codecContext, packet->InternalPacket);
        if (result == AVERROR(EAGAIN)) {
            // Output must be drained first, should not happen
            ReceiveSamples(output);
            result = avcodec_send_packet(_codecContext, packet->InternalPacket);
        }
        if (result < 0) {
            throw gcnew CodecException("Could not send a packet to the decoder: " + FFMpegException::DescribeError(result));
        }
        ReceiveSamples(output);
    }

    void AudioDecoder::Flush([NotNull] IList<AudioSamples^>^ output) {
        ThrowIfDisposed();
        if (output == nullptr) {
            throw gcnew ArgumentNullException("output");
        }
        if (_flushed || !_codecContext) {
            return;
        }
        _flushed = true;

        auto result = avcodec_send_packet(_codecContext, nullptr);
        if (result < 0 && result != AVERROR_EOF) {
            throw gcnew CodecException("Could not flush the decoder: " + FFMpegException::DescribeError(result));
        }
        ReceiveSamples(output);
    }

#pragma region IDisposable
    void AudioDecoder::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("AudioDecoder");
        }
    }

    AudioDecoder::~AudioDecoder() {
        this->!AudioDecoder();
        GC::SuppressFinalize(this);
    }

    AudioDecoder::!AudioDecoder() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_frame) {
            auto frame = _frame;
            av_frame_free(&frame);
            _frame = nullptr;
        }
        if (_codecContext) {
            auto context = _codecContext;
            avcodec_free_context(&context);
            _codecContext = nullptr;
        }
    }
#pragma endregion
}
