#include "pch.h"
#include "FileWriter.h"

#include <string>
#include <vcclr.h>
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {

    FileWriter::FileWriter()
        : _formatContext(nullptr)
        , _codecContext(nullptr)
        , _videoStream(nullptr)
        , _packet(av_packet_alloc())
        , _swsCtx(nullptr)
        , _convertedFrame(nullptr)
        , _filename(nullptr)
        , _initialized(false)
        , _timeBase(new AVRational())
        , _targetFormat(PixelFormat::YUV420P)
        , _targetWidth(0)
        , _targetHeight(0)
        , _gopSize(0)
        , _maxBFrames(0)
        , _lastPts(AV_NOPTS_VALUE)
        , _prevWidth(-1)
        , _prevHeight(-1)
        , _prevPixelFormat(AV_PIX_FMT_NONE)
        , _disposed(false) {
        
        // Set time base to ticks (10,000,000 ticks per second)
        _timeBase->num = 1;
        _timeBase->den = 10000000;
    }

    void FileWriter::WriteFrame([NotNull] Frame^ frame) {
        ThrowIfDisposed();
        
        if (frame == nullptr) {
            throw gcnew ArgumentNullException("frame");
        }
        
        // Initialize encoder on first frame
        if (!_initialized) {
            InitializeEncoder(frame->Format, frame->Width, frame->Height);
        }
        
        EncodeAndWriteFrame(frame);
    }

    void FileWriter::InitializeEncoder(PixelFormat frameFormat, int frameWidth, int frameHeight) {
        // Check if filename is set
        if (String::IsNullOrEmpty(_filename)) {
            throw gcnew InvalidOperationException("Filename must be set before writing frames");
        }
        
        // Determine actual encoding dimensions
        auto encodingWidth = _targetWidth;
        auto encodingHeight = _targetHeight;
        
        if (_targetWidth == 0 && _targetHeight == 0) {
            // Both dimensions are 0: use first frame's original dimensions
            encodingWidth = frameWidth;
            encodingHeight = frameHeight;
        } else if (_targetWidth == 0) {
            // Width is 0: scale proportionally based on target height
            auto aspectRatio = (double)frameWidth / frameHeight;
            encodingWidth = (int)(_targetHeight * aspectRatio);
            // Ensure even dimension for video encoding
            if (encodingWidth % 2 != 0) encodingWidth--;
            encodingHeight = _targetHeight;
        } else if (_targetHeight == 0) {
            // Height is 0: scale proportionally based on target width
            auto aspectRatio = (double)frameHeight / frameWidth;
            encodingHeight = (int)(_targetWidth * aspectRatio);
            // Ensure even dimension for video encoding
            if (encodingHeight % 2 != 0) encodingHeight--;
            encodingWidth = _targetWidth;
        }
        // If both dimensions are non-zero, use them as-is (encodingWidth and encodingHeight are already set)
        
        // Find NVENC HEVC encoder
        auto codec = avcodec_find_encoder_by_name("hevc_nvenc");
        if (!codec) {
            throw gcnew CodecException("NVENC HEVC encoder not found");
        }
        
        // Allocate codec context
        _codecContext = avcodec_alloc_context3(codec);
        if (!_codecContext) {
            throw gcnew CodecException("Could not allocate codec context");
        }
        
        // Set time base
        _codecContext->time_base = *_timeBase;
        
        // Set encoder dimensions
        _codecContext->width = encodingWidth;
        _codecContext->height = encodingHeight;
        _codecContext->pix_fmt = static_cast<AVPixelFormat>(_targetFormat == PixelFormat::None ? frameFormat : _targetFormat);
        
        // Set encoding parameters for real-time encoding
        _codecContext->codec_id = codec->id;
        _codecContext->codec_type = AVMEDIA_TYPE_VIDEO;
        _codecContext->gop_size = _gopSize; // the number of pictures in a group of pictures, or 0 for intra_only
        _codecContext->max_b_frames = _maxBFrames; // maximum number of B-frames between non-B-frames Note: The output will be delayed by max_b_frames+1 relative to the input.
        
        // Set NVENC lossless tuning
        auto ret = av_opt_set(_codecContext->priv_data, "preset", "lossless", 0);
        if (ret < 0) {
            throw gcnew CodecException("Failed to set NVENC preset");
        }

        // Convert filename to std::string
        auto filename = (String^)_filename;
        auto fname = msclr::interop::marshal_as<std::string>(filename);

        // Allocate output format context for MP4
        auto ctx = (AVFormatContext*)nullptr;
        if (avformat_alloc_output_context2(&ctx, nullptr, nullptr, fname.c_str()) < 0) {
            throw gcnew FFMpegException("Could not create output context for MP4 format");
        }
        _formatContext = ctx;
        
        // Create video stream
        _videoStream = avformat_new_stream(_formatContext, nullptr);
        if (!_videoStream) {
            throw gcnew FFMpegException("Could not create video stream");
        }
        
        _videoStream->id = _formatContext->nb_streams - 1;
        _videoStream->time_base = *_timeBase;
        
        // Open codec
        ret = avcodec_open2(_codecContext, nullptr, nullptr);
        if (ret < 0) {
            char errbuf[AV_ERROR_MAX_STRING_SIZE] = {0};
            av_strerror(ret, errbuf, sizeof(errbuf));
            String^ errorMessage = gcnew String(errbuf);
            throw gcnew CodecException("Could not open codec: " + errorMessage);
        }
        
        // Copy codec parameters to stream
        ret = avcodec_parameters_from_context(_videoStream->codecpar, _codecContext);
        if (ret < 0) {
            throw gcnew CodecException("Could not copy codec parameters");
        }
        
        // Allocate converted frame (only used when format/size conversion is needed)
        _convertedFrame = av_frame_alloc();
        if (!_convertedFrame) {
            throw gcnew OutOfMemoryException("Could not allocate converted frame");
        }
        
        _convertedFrame->format = _codecContext->pix_fmt;
        _convertedFrame->width = _codecContext->width;
        _convertedFrame->height = _codecContext->height;
        
        ret = av_frame_get_buffer(_convertedFrame, 0);
        if (ret < 0) {
            throw gcnew OutOfMemoryException("Could not allocate converted frame buffer");
        }

        // Open output file
        if (!(_formatContext->oformat->flags & AVFMT_NOFILE)) {
            ret = avio_open(&_formatContext->pb, fname.c_str(), AVIO_FLAG_WRITE);
            if (ret < 0) {
                throw gcnew FFMpegException("Could not open output file");
            }
        }
        
        // Write file header
        ret = avformat_write_header(_formatContext, nullptr);
        if (ret < 0) {
            throw gcnew FFMpegException("Could not write file header");
        }
        
        _initialized = true;
    }

    void FileWriter::EncodeAndWriteFrame([NotNull] Frame^ frame) {
        auto frameWidth = frame->Width;
        auto frameHeight = frame->Height;
        auto frameFormat = static_cast<AVPixelFormat>(frame->Format);
        
        // Calculate scaled dimensions maintaining aspect ratio using actual encoding dimensions
        auto scaledDimensions = CalculateScaledDimensions(frameWidth, frameHeight, _codecContext->width, _codecContext->height);
        auto scaledWidth = scaledDimensions.Item1;
        auto scaledHeight = scaledDimensions.Item2;
        
        // Check if we need to recreate SwsContext
        if (frameWidth != _prevWidth || frameHeight != _prevHeight || frameFormat != _prevPixelFormat) {
            if (_swsCtx) {
                sws_freeContext(_swsCtx);
                _swsCtx = nullptr;
            }
            
            _swsCtx = sws_getContext(
                frameWidth, frameHeight, frameFormat,
                scaledWidth, scaledHeight, _codecContext->pix_fmt,
                SWS_BILINEAR,
                nullptr, nullptr, nullptr
            );
            
            if (!_swsCtx) {
                throw gcnew CodecException("Failed to create SwsContext for frame conversion");
            }
            
            _prevWidth = frameWidth;
            _prevHeight = frameHeight;
            _prevPixelFormat = frameFormat;
        }
        
        // Determine which frame to send to encoder
        auto frameToEncode = (AVFrame*)nullptr;
        
        // Check if conversion is needed
        auto needsConversion = 
            frameFormat != _codecContext->pix_fmt 
            || scaledWidth != _codecContext->width
            || scaledHeight != _codecContext->height;
        
        if (!needsConversion) {
            frameToEncode = frame->InternalAVFrame;
        } else {
            frameToEncode = _convertedFrame;
            sws_scale(
                _swsCtx,
                frame->InternalAVFrame->data,
                frame->InternalAVFrame->linesize,
                0, frameHeight,
                _convertedFrame->data, _convertedFrame->linesize
            );
        }
        
        // Set frame timestamp from input frame for variable frame rate support
        // Use raw PTS value from input frame to preserve precise timing
        auto inputPts = frame->PTS;
        
        // Validate PTS for variable frame rate encoding
        if (inputPts == AV_NOPTS_VALUE) {
            throw gcnew ArgumentException("Frame PTS value is invalid. Variable frame rate encoding requires valid timestamps.", "frame");
        }
        
        // For variable frame rate, ensure PTS is monotonically increasing
        if (_lastPts != AV_NOPTS_VALUE && inputPts <= _lastPts) {
            throw gcnew ArgumentException(String::Format("Frame PTS ({0}) must be greater than previous frame PTS ({1}) for variable frame rate encoding.", inputPts, _lastPts), "frame");
        }
        
        frameToEncode->pts = inputPts;
        _lastPts = inputPts;
        
        // Encode frame
        auto ret = avcodec_send_frame(_codecContext, frameToEncode);
        if (ret < 0) {
            throw gcnew CodecException("Error sending frame to encoder");
        }
        
        // Get encoded packets and write them
        ReceiveAndWritePackets(true);
    }

    ValueTuple<int, int> FileWriter::CalculateScaledDimensions(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight) {
        // Calculate aspect ratios
        auto sourceAspect = (double)sourceWidth / sourceHeight;
        auto targetAspect = (double)targetWidth / targetHeight;
        
        auto outputWidth = targetWidth;
        auto outputHeight = targetHeight;
        
        if (sourceAspect > targetAspect) {
            // Source is wider, fit by width
            outputWidth = targetWidth;
            outputHeight = (int)(targetWidth / sourceAspect);
            // Ensure even dimensions for video encoding
            if (outputHeight % 2 != 0) outputHeight--;
        } else {
            // Source is taller, fit by height
            outputHeight = targetHeight;
            outputWidth = (int)(targetHeight * sourceAspect);
            // Ensure even dimensions for video encoding
            if (outputWidth % 2 != 0) outputWidth--;
        }
        
        return ValueTuple<int, int>(outputWidth, outputHeight);
    }

    void FileWriter::ReceiveAndWritePackets(bool throwOnError) {
        auto packetRet = 0;
        while (packetRet >= 0) {
            packetRet = avcodec_receive_packet(_codecContext, _packet);
            if (packetRet == AVERROR(EAGAIN) || packetRet == AVERROR_EOF) {
                break;
            } else if (packetRet < 0) {
                if (throwOnError) {
                    throw gcnew CodecException("Error receiving packet from encoder");
                }
                break;
            }
            
            // Scale packet timestamps
            av_packet_rescale_ts(_packet, _codecContext->time_base, _videoStream->time_base);
            _packet->stream_index = _videoStream->index;
            
            // Write packet
            auto writeRet = av_interleaved_write_frame(_formatContext, _packet);
            if (writeRet < 0 && throwOnError) {
                char errbuf[AV_ERROR_MAX_STRING_SIZE] = { 0 };
                av_strerror(writeRet, errbuf, sizeof(errbuf));
                String^ errorMessage = gcnew String(errbuf);
                throw gcnew FFMpegException("Error writing packet to file: " + errorMessage);
            }
            
            av_packet_unref(_packet);
        }
    }

#pragma region IDisposable
    void FileWriter::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("FileWriter");
        }
    }

    FileWriter::~FileWriter() {
        this->!FileWriter();
        GC::SuppressFinalize(this);
    }

    FileWriter::!FileWriter() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        // Flush encoder
        if (_codecContext && _initialized) {
            avcodec_send_frame(_codecContext, nullptr);
            
            ReceiveAndWritePackets(false); // Don't throw during cleanup
        }
        
        // Write file trailer
        if (_formatContext && _initialized) {
            av_write_trailer(_formatContext);
        }
        
        // Clean up resources
        if (_convertedFrame) {
            auto tmp = _convertedFrame;
            av_frame_free(&tmp);
            _convertedFrame = nullptr;
        }
        
        if (_swsCtx) {
            sws_freeContext(_swsCtx);
            _swsCtx = nullptr;
        }
        
        if (_packet) {
            auto tmp = _packet;
            av_packet_free(&tmp);
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
            if (_formatContext->oformat && !(_formatContext->oformat->flags & AVFMT_NOFILE)) {
                avio_closep(&_formatContext->pb);
            }
            auto tmp = _formatContext;
            avformat_free_context(tmp);
            _formatContext = nullptr;
        }
    }
#pragma endregion

}
