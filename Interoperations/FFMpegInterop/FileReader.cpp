#include "pch.h"
#include "FileReader.h"

#include <string>

using namespace System;
using namespace System::Buffers;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {

    FileReader::FileReader(String^ filename) 
        : _unmanaged(new FileReaderUnmanaged()) {

        // Convert System::String to std::string
        std::string fname = msclr::interop::marshal_as<std::string>(filename);

        // Open video file
        if (avformat_open_input(&_unmanaged->formatContext, fname.c_str(), nullptr, nullptr) != 0) {
            throw gcnew System::Exception("Could not open file.");
        }

        // Retrieve stream information
        if (avformat_find_stream_info(_unmanaged->formatContext, nullptr) < 0) {
            throw gcnew System::Exception("Could not find stream information.");
        }

        // Find the first video stream
        for (unsigned i = 0; i < _unmanaged->formatContext->nb_streams; i++) {
            if (_unmanaged->formatContext->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
                videoStreamIndex = i;
                break;
            }
        }

        if (videoStreamIndex == -1) {
            throw gcnew System::Exception("Could not find a video stream.");
        }

        // Get codec parameters for the video stream
        AVCodecParameters* codecParameters = _unmanaged->formatContext->streams[videoStreamIndex]->codecpar;

        // Find the decoder for the video stream
        const AVCodec* codec = avcodec_find_decoder(codecParameters->codec_id);
        if (!codec) {
            throw gcnew System::Exception("Unsupported codec.");
        }

        // Allocate a codec context for the decoder
        _unmanaged->codecContext = avcodec_alloc_context3(codec);
        if (!_unmanaged->formatContext) {
            throw gcnew System::Exception("Could not allocate codec context.");
        }

        // Copy codec parameters from input stream to output codec context
        if (avcodec_parameters_to_context(_unmanaged->codecContext, codecParameters) < 0) {
            throw gcnew System::Exception("Failed to copy codec parameters.");
        }

        // Open the codec
        if (avcodec_open2(_unmanaged->codecContext, codec, nullptr) < 0) {
            throw gcnew System::Exception("Could not open codec.");
        }

        // Compute time base
        timeBase = av_q2d(_unmanaged->formatContext->streams[videoStreamIndex]->time_base);//Not from the frame, otherwise will get 0

        // Allocate
        _unmanaged->packet = new AVPacket();
        _unmanaged->rawFrame = av_frame_alloc();
        _unmanaged->convertedFrame = av_frame_alloc();
    }

    void FileReader::ReadOneFrame(Func<FrameInfo, ValueTuple<IntPtr, int>>^ allocator, bool% success, bool% eof) {
        success = false;
        eof = false;
        if (av_read_frame(_unmanaged->formatContext, _unmanaged->packet) < 0) {
            eof = true;
            return;
        }
        try {
            if (_unmanaged->packet->stream_index != videoStreamIndex) {
                return;
            }

            int send_response = avcodec_send_packet(_unmanaged->codecContext, _unmanaged->packet);
            if (send_response < 0) {
                throw gcnew System::Exception("Error while sending packet to the decoder.");
            }

            int receive_response = avcodec_receive_frame(_unmanaged->codecContext, _unmanaged->rawFrame);

            switch (receive_response) {
            case 0:
                break;
            case AVERROR(EAGAIN):
                return;
            default:
                throw gcnew System::Exception("Error while decoding frame.");
            }

            if (_unmanaged->rawFrame->pts == AV_NOPTS_VALUE) {
                throw gcnew System::Exception("PTS is not available.");
            }

            TimeSpan timestamp = TimeSpan::FromSeconds(_unmanaged->rawFrame->pts * timeBase);

            int width = _unmanaged->rawFrame->width;
            int height = _unmanaged->rawFrame->height;
            FrameInfo info = FrameInfo(timestamp, width, height, _unmanaged->rawFrame->key_frame);
            auto tuple = allocator(info);
            byte* dstData = static_cast<byte*>(tuple.Item1.ToPointer());
            int dstLength = tuple.Item2;
            if (dstData == nullptr || dstLength <= 0) {//skip this frame
                return;
            }

            byte* srcData = nullptr;
            int srcLength = 0;
            if (targetFormat == static_cast<AVPixelFormat>(_unmanaged->rawFrame->format)) {
                srcData = static_cast<byte*>(_unmanaged->rawFrame->data[0]);
                srcLength = ComputeBufferSize(*_unmanaged->rawFrame);
            } else {
                if (width != prevWidth || height != prevHeight) {
                    prevWidth = width;
                    prevHeight = height;

                    if (_unmanaged->swsCtx) {
                        sws_freeContext(_unmanaged->swsCtx);
                        _unmanaged->swsCtx = nullptr;
                    }
                    if (_unmanaged->convertedFrame) {
                        av_frame_free(&_unmanaged->convertedFrame);
                        _unmanaged->convertedFrame = nullptr;
                    }

                    _unmanaged->swsCtx = sws_getContext(
                        width, height, static_cast<AVPixelFormat>(_unmanaged->rawFrame->format),
                        width, height,
                        targetFormat,
                        SWS_BILINEAR,
                        nullptr, nullptr, nullptr
                    );

                    if (!_unmanaged->swsCtx) {
                        throw gcnew System::Exception("Failed to create swsContext for pixel format conversion.");
                    }

                    _unmanaged->convertedFrame = av_frame_alloc();
                    _unmanaged->convertedFrame->format = targetFormat;
                    _unmanaged->convertedFrame->width = width;
                    _unmanaged->convertedFrame->height = height;
                    av_frame_get_buffer(_unmanaged->convertedFrame, 0);
                }
                sws_scale(
                    _unmanaged->swsCtx,
                    _unmanaged->rawFrame->data,
                    _unmanaged->rawFrame->linesize,
                    0,
                    _unmanaged->rawFrame->height,
                    _unmanaged->convertedFrame->data,
                    _unmanaged->convertedFrame->linesize
                );

                srcData = static_cast<byte*>(_unmanaged->convertedFrame->data[0]);
                srcLength = ComputeBufferSize(*_unmanaged->convertedFrame);
            }

            if (srcLength != dstLength) {
                throw gcnew System::Exception("Buffer sizes mismatch!");
            }
            std::memcpy(dstData, srcData, srcLength);//I heard C++/CLR does not support Span<T>, so we use raw pointer here

            success = true;
        } finally {
            av_packet_unref(_unmanaged->packet);
        }
    }

    int FileReader::ComputeBufferSize(const AVFrame& frame) {
        int bufferSize = 0;

        switch (frame.format) {
        case AV_PIX_FMT_YUV420P:
            bufferSize = frame.linesize[0] * frame.height; // Y plane
            bufferSize += frame.linesize[1] * (frame.height / 2); // U plane
            bufferSize += frame.linesize[2] * (frame.height / 2); // V plane
            break;
        case AV_PIX_FMT_YUV444P:
            bufferSize = frame.linesize[0] * frame.height; // Y plane
            bufferSize += frame.linesize[1] * frame.height; // U plane
            bufferSize += frame.linesize[2] * frame.height; // V plane
            break;
        case AV_PIX_FMT_BGR24:
        case AV_PIX_FMT_RGB24:
        case AV_PIX_FMT_BGRA:
        case AV_PIX_FMT_GRAY8:
        case AV_PIX_FMT_GRAY16LE:
        case AV_PIX_FMT_RGBA64LE:
            bufferSize = frame.linesize[0] * frame.height; // BGR plane
            break;
        default:
            throw gcnew System::Exception("Unsupported pixel format.");
        }

        return bufferSize;
    }


#pragma region IDisposable
    FileReader::~FileReader() {
        if (disposed) {
            return;
        }
        disposed = true;
        this->!FileReader();
    }

    FileReader::!FileReader() {
        av_frame_free(&_unmanaged->rawFrame);
        av_frame_free(&_unmanaged->convertedFrame);

        if (_unmanaged->swsCtx) {
            sws_freeContext(_unmanaged->swsCtx);
        }

        delete _unmanaged->packet;

        if (_unmanaged->codecContext) {
            avcodec_close(_unmanaged->codecContext);
            avcodec_free_context(&_unmanaged->codecContext);
        }

        if (_unmanaged->formatContext) {
            avformat_close_input(&_unmanaged->formatContext);
        }

        delete _unmanaged;
    }
#pragma endregion
}