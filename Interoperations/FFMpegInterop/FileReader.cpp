#include "pch.h"
#include "FileReader.h"

#include <string>

using namespace System;
using namespace System::Buffers;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {

    FileReader::FileReader(String^ filename, Func<FrameInfo, ValueTuple<IntPtr, int, Action^>>^ callback) : _unmanaged(new FileReaderUnmanaged()), _callback(callback) {

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
    }

    void FileReader::Run() {
        double timeBase = av_q2d(_unmanaged->formatContext->streams[videoStreamIndex]->time_base);//Not from the frame

        AVPacket packet;
        SwsContext* swsCtx = nullptr;
        int prevWidth = -1;
        int prevHeight = -1;

        AVFrame* rawFrame = av_frame_alloc();
        AVFrame* convertedFrame = av_frame_alloc();
        try {
            while (av_read_frame(_unmanaged->formatContext, &packet) >= 0) {
                try {
                    if (packet.stream_index != videoStreamIndex) {
                        continue;
                    }

                    int send_response = avcodec_send_packet(_unmanaged->codecContext, &packet);
                    if (send_response < 0) {
                        throw gcnew System::Exception("Error while sending packet to the decoder.");
                    }

                    int receive_response = avcodec_receive_frame(_unmanaged->codecContext, rawFrame);

                    switch (receive_response) {
                    case 0:
                        break;
                    case AVERROR(EAGAIN):
                        continue;
                    default:
                        throw gcnew System::Exception("Error while decoding frame.");
                    }

                    if (rawFrame->pts == AV_NOPTS_VALUE) {
                        throw gcnew System::Exception("PTS is not available.");
                    }
                    TimeSpan timestamp = TimeSpan::FromSeconds(rawFrame->pts * timeBase);

                    FrameInfo info = FrameInfo(timestamp, rawFrame->width, rawFrame->height, rawFrame->key_frame);
                    auto tuple = _callback(info);

                    byte* srcData = nullptr;
                    int srcLength = 0;
                    if (_targetPixelFormat == static_cast<AVPixelFormat>(rawFrame->format)) {
                        srcData = static_cast<byte*>(rawFrame->data[0]);
                        srcLength = ComputeBufferSize(*rawFrame);
                    } else {
                        if (rawFrame->width != prevWidth || rawFrame->height != prevHeight) {
                            prevWidth = rawFrame->width;
                            prevHeight = rawFrame->height;

                            if (swsCtx) {
                                sws_freeContext(swsCtx);
                                swsCtx = nullptr;
                            }
                            if (convertedFrame) {
                                av_frame_free(&convertedFrame);
                                convertedFrame = nullptr;
                            }

                            swsCtx = sws_getContext(
                                rawFrame->width, rawFrame->height, static_cast<AVPixelFormat>(rawFrame->format),
                                rawFrame->width, rawFrame->height,
                                _targetPixelFormat,
                                SWS_BILINEAR,
                                nullptr, nullptr, nullptr
                            );

                            if (!swsCtx) {
                                throw gcnew System::Exception("Failed to create swsContext for pixel format conversion.");
                            }

                            convertedFrame = av_frame_alloc();
                            convertedFrame->format = _targetPixelFormat;
                            convertedFrame->width = rawFrame->width;
                            convertedFrame->height = rawFrame->height;
                            av_frame_get_buffer(convertedFrame, 0);
                        }
                        sws_scale(
                            swsCtx,
                            rawFrame->data,
                            rawFrame->linesize,
                            0,
                            rawFrame->height,
                            convertedFrame->data,
                            convertedFrame->linesize
                        );

                        srcData = static_cast<byte*>(convertedFrame->data[0]);
                        srcLength = ComputeBufferSize(*convertedFrame);
                    }
                    
                    byte* destData = static_cast<byte*>(tuple.Item1.ToPointer());
                    int dstLength = tuple.Item2;
                    if (srcLength != dstLength) {
                        throw gcnew System::Exception("Buffer sizes mismatch!");
                    }
                    std::memcpy(destData, srcData, srcLength);//I heard C++/CLR does not support Span<T>

                    tuple.Item3();
                } finally {
                    av_packet_unref(&packet);
                }
            }
        } finally {
            av_frame_free(&rawFrame);
            av_frame_free(&convertedFrame);
            if (swsCtx) {
                sws_freeContext(swsCtx);
            }
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