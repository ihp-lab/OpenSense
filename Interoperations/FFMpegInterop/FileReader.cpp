#include "pch.h"
#include "FileReader.h"

#include <string>
#include <vcclr.h>

using namespace System;
using namespace System::Buffers;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {

    // FileReader implementation
    FileReader::FileReader(String^ filename)
        : _formatContext(nullptr)
        , _codecContext(nullptr)
        , _packet(new AVPacket())
        , _swsCtx(nullptr)
        , _rawFrame(av_frame_alloc())
        , _convertedFrame(av_frame_alloc())
        , _videoStreamIndex(-1)
        , _timeBase(new AVRational())
        , _targetFormat(PixelFormat::None)
        , _targetWidth(0)
        , _targetHeight(0)
        , _onlyKeyFrames(false)
        , _prevWidth(-1)
        , _prevHeight(-1)
        , _prevSrcFormat(AVPixelFormat::AV_PIX_FMT_NONE)
        , _prevDstFormat(AVPixelFormat::AV_PIX_FMT_NONE)
        , _currentFrame(nullptr)
        , _endOfFile(false)
        , _started(false)
        , _disposed(false) {

        // Convert System::String to std::string
        auto fname = msclr::interop::marshal_as<std::string>(filename);

        // Open video file
        auto ctx = (AVFormatContext*)nullptr;
        if (avformat_open_input(&ctx, fname.c_str(), nullptr, nullptr) != 0) {
            throw gcnew FileOpenException(filename);
        }
        this->_formatContext = ctx;

        // Retrieve stream information
        if (avformat_find_stream_info(_formatContext, nullptr) < 0) {
            throw gcnew FileReaderException("Could not find stream information.");
        }

        // Find the first video stream
        auto videoStreamIndex = -1;
        for (auto i = 0u; i < _formatContext->nb_streams; i++) {
            if (_formatContext->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
                videoStreamIndex = static_cast<int>(i);
                break;
            }
        }

        if (videoStreamIndex == -1) {
            throw gcnew FileReaderException("Could not find a video stream.");
        }

        // Set the video stream index and time base
        _videoStreamIndex = videoStreamIndex;
        *_timeBase = _formatContext->streams[_videoStreamIndex]->time_base;

        // Initialize codec
        auto codecParameters = _formatContext->streams[_videoStreamIndex]->codecpar;

        // Find the decoder for the video stream
        auto codec = avcodec_find_decoder(codecParameters->codec_id);
        if (!codec) {
            throw gcnew CodecException("Unsupported codec.");
        }

        // Allocate a codec context for the decoder
        _codecContext = avcodec_alloc_context3(codec);
        if (!_codecContext) {
            throw gcnew CodecException("Could not allocate codec context.");
        }

        // Copy codec parameters from input stream to output codec context
        if (avcodec_parameters_to_context(_codecContext, codecParameters) < 0) {
            throw gcnew CodecException("Failed to copy codec parameters.");
        }

        // Open the codec
        if (avcodec_open2(_codecContext, codec, nullptr) < 0) {
            throw gcnew CodecException("Could not open codec.");
        }
    }

    Frame^ FileReader::ReadNextFrame() {
        if (_endOfFile) {
            return nullptr;
        }

        while (true) {
            if (av_read_frame(_formatContext, _packet) < 0) {
                _endOfFile = true;
                return nullptr;
            }

            try {
                if (_packet->stream_index != _videoStreamIndex) {
                    continue;
                }

                auto send_response = avcodec_send_packet(_codecContext, _packet);
                if (send_response < 0) {
                    throw gcnew CodecException("Error while sending packet to the decoder.");
                }

                auto receive_response = avcodec_receive_frame(_codecContext, _rawFrame);

                switch (receive_response) {
                case 0:
                    break;
                case AVERROR(EAGAIN):
                    continue;
                default:
                    throw gcnew CodecException("Error while decoding frame.");
                }

                if (_rawFrame->pts == AV_NOPTS_VALUE) {
                    throw gcnew FileReaderException("PTS is not available.");
                }

                // Calculate timestamp: pts * timeBase
                auto timestamp = TimeSpan::FromSeconds(_rawFrame->pts * av_q2d(*_timeBase));
                auto width = _rawFrame->width;
                auto height = _rawFrame->height;
                auto keyFrame = _rawFrame->key_frame;

                // Skip non-key frames if OnlyKeyFrames is true
                if (_onlyKeyFrames && !keyFrame) {
                    continue;
                }

                auto originalFormat = static_cast<AVPixelFormat>(_rawFrame->format);
                auto targetFormat = static_cast<AVPixelFormat>(_targetFormat);
                auto outputWidth = width;
                auto outputHeight = height;
                auto outputFormat = originalFormat;
                auto needsFormatConversion = targetFormat != AVPixelFormat::AV_PIX_FMT_NONE && targetFormat != originalFormat;
                CalculateOutputDimensions(width, height, _targetWidth, _targetHeight, outputWidth, outputHeight);
                auto needsScaling = width != outputWidth || height != outputHeight;
                auto needsConversion = needsFormatConversion || needsScaling;

                if (!needsConversion) {
                    auto isMultiPlane = IsMultiPlaneFormat(originalFormat);
                    if (!isMultiPlane) {
                        auto srcData = static_cast<byte*>(_rawFrame->data[0]);
                        auto srcLength = _rawFrame->linesize[0] * _rawFrame->height;
                        auto data = gcnew array<Byte>(srcLength);
                        Marshal::Copy(IntPtr(srcData), data, 0, srcLength);
                        // Create and return frame directly
                        return gcnew Frame(timestamp, width, height, keyFrame, static_cast<PixelFormat>(originalFormat), data);
                    }

                    auto srcLength = CalculateMultiPlaneBufferSize(_rawFrame, originalFormat);
                    auto data = gcnew array<Byte>(srcLength);
                    auto offset = 0;
                    auto ySize = _rawFrame->linesize[0] * _rawFrame->height;
                    Marshal::Copy(IntPtr(_rawFrame->data[0]), data, offset, ySize);
                    offset += ySize;
                    if (!_rawFrame->data[1]) {
                        throw gcnew FileReaderException("U plane data is null for multi-plane format.");
                    }
                    auto uHeight = _rawFrame->height / GetVerticalSubsamplingDivisor(originalFormat);
                    auto uSize = _rawFrame->linesize[1] * uHeight;
                    Marshal::Copy(IntPtr(_rawFrame->data[1]), data, offset, uSize);
                    offset += uSize;
                    if (!_rawFrame->data[2]) {
                        throw gcnew FileReaderException("V plane data is null for multi-plane format.");
                    }
                    auto vHeight = _rawFrame->height / GetVerticalSubsamplingDivisor(originalFormat);
                    auto vSize = _rawFrame->linesize[2] * vHeight;
                    Marshal::Copy(IntPtr(_rawFrame->data[2]), data, offset, vSize);
                    return gcnew Frame(timestamp, width, height, keyFrame, static_cast<PixelFormat>(originalFormat), data);
                }


                if (needsFormatConversion) {
                    outputFormat = targetFormat;
                }

                if (outputWidth != _prevWidth || outputHeight != _prevHeight || originalFormat != _prevSrcFormat || outputFormat != _prevDstFormat) {
                    _prevWidth = outputWidth;
                    _prevHeight = outputHeight;
                    _prevSrcFormat = originalFormat;
                    _prevDstFormat = outputFormat;
                    if (_swsCtx) {
                        sws_freeContext(_swsCtx);
                        _swsCtx = nullptr;
                    }
                    if (_convertedFrame) {
                        auto tmp = _convertedFrame;
                        av_frame_free(&tmp);
                        _convertedFrame = nullptr;
                    }
                    _swsCtx = sws_getContext(
                        width, height, originalFormat,
                        outputWidth, outputHeight, outputFormat,
                        SWS_BILINEAR,
                        nullptr, nullptr, nullptr
                    );
                    if (!_swsCtx) {
                        throw gcnew CodecException("Failed to create swsContext for format/size conversion.");
                    }
                    _convertedFrame = av_frame_alloc();
                    _convertedFrame->format = outputFormat;
                    _convertedFrame->width = outputWidth;
                    _convertedFrame->height = outputHeight;
                    av_frame_get_buffer(_convertedFrame, 0);
                }

                sws_scale(
                    _swsCtx,
                    _rawFrame->data,
                    _rawFrame->linesize,
                    0,
                    _rawFrame->height,
                    _convertedFrame->data,
                    _convertedFrame->linesize
                );

                auto srcData = static_cast<byte*>(_convertedFrame->data[0]);
                auto srcLength = _convertedFrame->linesize[0] * _convertedFrame->height;
                auto data = gcnew array<Byte>(srcLength);
                Marshal::Copy(IntPtr(srcData), data, 0, srcLength);
                return gcnew Frame(timestamp, outputWidth, outputHeight, keyFrame, static_cast<PixelFormat>(outputFormat), data);
            } finally {
                av_packet_unref(_packet);
            }
        }
    }

    bool FileReader::IsMultiPlaneFormat(AVPixelFormat format) {
        switch (format) {
        case AV_PIX_FMT_YUV420P:
        case AV_PIX_FMT_YUV444P:
        case AV_PIX_FMT_YUV422P:
        case AV_PIX_FMT_YUV411P:
        case AV_PIX_FMT_YUV410P:
            //Not a complete list
            return true;
        default:
            return false;
        }
    }

    void FileReader::CalculateOutputDimensions(int originalWidth, int originalHeight, int targetWidth, int targetHeight, int& outputWidth, int& outputHeight) {
        if (targetWidth > 0 && targetHeight > 0) {
            // Both dimensions specified
            outputWidth = targetWidth;
            outputHeight = targetHeight;
        } else if (targetWidth > 0) {
            // Only width specified, maintain aspect ratio
            outputWidth = targetWidth;
            outputHeight = (int)((double)originalHeight * targetWidth / originalWidth);
        } else if (targetHeight > 0) {
            // Only height specified, maintain aspect ratio
            outputWidth = (int)((double)originalWidth * targetHeight / originalHeight);
            outputHeight = targetHeight;
        } else {
            // No scaling
            outputWidth = originalWidth;
            outputHeight = originalHeight;
        }
    }

    int FileReader::GetVerticalSubsamplingDivisor(AVPixelFormat format) {
        switch (format) {
        case AV_PIX_FMT_YUV420P:
            return 2;  // 4:2:0 subsampling
        case AV_PIX_FMT_YUV410P:
            return 4;  // 4:1:0 subsampling  
        case AV_PIX_FMT_YUV444P:
        case AV_PIX_FMT_YUV422P:
        case AV_PIX_FMT_YUV411P:
        default:
            return 1;  // No vertical subsampling
        }
    }

    int FileReader::CalculateMultiPlaneBufferSize(AVFrame* frame, AVPixelFormat format) {
        auto totalSize = frame->linesize[0] * frame->height; // Y plane

        // U and V planes have the same height calculation
        auto chromaHeight = frame->height / GetVerticalSubsamplingDivisor(format);
        for (int i = 1; i <= 2; i++) {
            if (!frame->data[i]) {
                throw gcnew FileReaderException("Chroma plane data is null for multi-plane format.");
            }
            totalSize += frame->linesize[i] * chromaHeight;
        }

        return totalSize;
    }

#pragma region IEnumerator<Frame^> implementation
    Frame^ FileReader::Current::get() {
        if (_currentFrame == nullptr) {
            throw gcnew InvalidOperationException("No current frame available");
        }
        return _currentFrame;
    }

    Object^ FileReader::Current2::get() {
        return Current;
    }

    bool FileReader::MoveNext() {
        ThrowIfDisposed();
        _currentFrame = ReadNextFrame();
        return _currentFrame != nullptr;
    }

    void FileReader::Reset() {
        ThrowIfDisposed();

        // Seek to beginning of file
        if (av_seek_frame(_formatContext, _videoStreamIndex, 0, AVSEEK_FLAG_BACKWARD) < 0) {
            throw gcnew FileReaderException("Could not seek to beginning of file.");
        }

        _currentFrame = nullptr;
        _endOfFile = false;
        _started = false;
    }
#pragma endregion

#pragma region IDisposable implementation
    void FileReader::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("FileReader");
        }
    }

    FileReader::~FileReader() {
        this->!FileReader();
        GC::SuppressFinalize(this);
    }

    FileReader::!FileReader() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_convertedFrame) {
            auto tmp = _convertedFrame;//create a raw pointer on stack, because getting the address of a field of a managed object will return interior_ptr<>.
            av_frame_free(&tmp);
            _convertedFrame = nullptr;
        }
        if (_rawFrame) {
            auto tmp = _rawFrame;
            av_frame_free(&tmp);
            _rawFrame = nullptr;
        }
        if (_swsCtx) {
            sws_freeContext(_swsCtx);
        }
        if (_packet) {
            delete _packet;
            _packet = nullptr;
        }
        if (_timeBase) {
            delete _timeBase;
            _timeBase = nullptr;
        }
        if (_codecContext) {
            avcodec_close(_codecContext);
            auto tmp = _codecContext;
            avcodec_free_context(&tmp);
            _codecContext = nullptr;
        }
        if (_formatContext) {
            auto tmp = _formatContext;
            avformat_close_input(&tmp);
            _formatContext = nullptr;
        }
    }
#pragma endregion

}