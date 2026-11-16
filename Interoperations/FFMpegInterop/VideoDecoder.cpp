#include "pch.h"

#include "VideoDecoder.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    namespace {
        const AVRational TicksTimeBase = { 1, 10000000 };
    }

    VideoDecoder::VideoDecoder()
        : _codecContext(nullptr)
        , _frame(nullptr)
        , _flushed(false)
        , _disposed(false) {
    }

    void VideoDecoder::Open(CodecParameters^ parameters) {
        if (parameters->MediaType != MediaType::Video) {
            throw gcnew ArgumentException("The packet does not belong to a video stream.", "packet");
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
        // Slice threading only, frame threading delays output by as many frames as threads
        _codecContext->thread_type = FF_THREAD_SLICE;
        // 0 = auto
        _codecContext->thread_count = 0;
        result = avcodec_open2(_codecContext, codec, nullptr);
        if (result < 0) {
            throw gcnew CodecException("Could not open the decoder: " + FFMpegException::DescribeError(result));
        }
        _frame = av_frame_alloc();
        if (!_frame) {
            throw gcnew OutOfMemoryException("Could not allocate a frame.");
        }
    }

    void VideoDecoder::ReceiveFrames(IList<Frame^>^ output) {
        while (true) {
            auto result = avcodec_receive_frame(_codecContext, _frame);
            if (result == AVERROR(EAGAIN) || result == AVERROR_EOF) {
                return;
            }
            if (result < 0) {
                throw gcnew CodecException("Could not decode a frame: " + FFMpegException::DescribeError(result));
            }
            auto presentationTime = _frame->pts != AV_NOPTS_VALUE ? _frame->pts : _frame->best_effort_timestamp;
            if (presentationTime == AV_NOPTS_VALUE) {
                av_frame_unref(_frame);
                throw gcnew CodecException("The decoder returned a frame without a timestamp.");
            }
            auto frame = av_frame_alloc();
            if (!frame) {
                av_frame_unref(_frame);
                throw gcnew OutOfMemoryException("Could not allocate a frame.");
            }
            av_frame_move_ref(frame, _frame);
            frame->pts = presentationTime;
            frame->time_base = TicksTimeBase;
            output->Add(gcnew Frame(frame));
        }
    }

    void VideoDecoder::Decode([NotNull] Packet^ packet, [NotNull] IList<Frame^>^ output) {
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
            ReceiveFrames(output);
            result = avcodec_send_packet(_codecContext, packet->InternalPacket);
        }
        if (result < 0) {
            throw gcnew CodecException("Could not send a packet to the decoder: " + FFMpegException::DescribeError(result));
        }
        ReceiveFrames(output);
    }

    void VideoDecoder::Flush([NotNull] IList<Frame^>^ output) {
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
        ReceiveFrames(output);
    }

#pragma region IDisposable
    void VideoDecoder::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("VideoDecoder");
        }
    }

    VideoDecoder::~VideoDecoder() {
        this->!VideoDecoder();
        GC::SuppressFinalize(this);
    }

    VideoDecoder::!VideoDecoder() {
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
