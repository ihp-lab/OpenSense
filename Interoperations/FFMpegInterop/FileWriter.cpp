#include "pch.h"
#include "FileWriter.h"

#include <string>
#include <vcclr.h>
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {

    FileWriter::FileWriter([NotNull] String^ filename, int width, int height)
        : _formatContext(nullptr)
        , _codecContext(nullptr)
        , _videoStream(nullptr)
        , _packet(av_packet_alloc())
        , _swsCtx(nullptr)
        , _convertedFrame(nullptr)
        , _filename(filename)
        , _initialized(false)
        , _timeBase(new AVRational())
        , _targetWidth(width)
        , _targetHeight(height)
        , _lastPts(AV_NOPTS_VALUE)
        , _prevWidth(-1)
        , _prevHeight(-1)
        , _prevPixelFormat(AV_PIX_FMT_NONE)
        , _disposed(false) {
        
        if (String::IsNullOrEmpty(filename)) {
            throw gcnew ArgumentNullException("filename");
        }
        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }
        
        // Convert filename to std::string
        auto fname = msclr::interop::marshal_as<std::string>(filename);
        
        // Allocate output format context for MP4
        auto ctx = (AVFormatContext*)nullptr;
        if (avformat_alloc_output_context2(&ctx, nullptr, nullptr, fname.c_str()) < 0) {
            throw gcnew FFMpegException("Could not create output context for MP4 format");
        }
        _formatContext = ctx;
        
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
        
        // Set time base to ticks (10,000,000 ticks per second)
        _timeBase->num = 1;
        _timeBase->den = 10000000;
        _codecContext->time_base = *_timeBase;
        
        // Set encoder dimensions
        _codecContext->width = _targetWidth;
        _codecContext->height = _targetHeight;
        _codecContext->pix_fmt = AV_PIX_FMT_YUV420P; // Standard format for H.265
        
        // Set encoding parameters for real-time encoding
        _codecContext->codec_id = codec->id;
        _codecContext->codec_type = AVMEDIA_TYPE_VIDEO;
        _codecContext->gop_size = 1; // No B-frames, real-time encoding
        _codecContext->max_b_frames = 0;
        
        // Set NVENC lossless tuning
        av_opt_set(_codecContext->priv_data, "preset", "lossless", 0);
    }

    void FileWriter::WriteFrame([NotNull] Frame^ frame) {
        ThrowIfDisposed();
        
        if (frame == nullptr) {
            throw gcnew ArgumentNullException("frame");
        }
        
        // Validate pixel format
        if (!PixelFormatHelper::IsSupported(frame->Format)) {
            throw gcnew ArgumentException("Unsupported pixel format", "frame");
        }
        
        // Initialize encoder on first frame
        if (!_initialized) {
            InitializeEncoder();
        }
        
        EncodeAndWriteFrame(frame);
    }

    void FileWriter::InitializeEncoder() {
        // Encoder dimensions are already set in constructor
        
        // Create video stream
        _videoStream = avformat_new_stream(_formatContext, nullptr);
        if (!_videoStream) {
            throw gcnew FFMpegException("Could not create video stream");
        }
        
        _videoStream->id = _formatContext->nb_streams - 1;
        _videoStream->time_base = *_timeBase;
        
        // Open codec
        auto ret = avcodec_open2(_codecContext, nullptr, nullptr);
        if (ret < 0) {
            throw gcnew CodecException("Could not open codec");
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
        String^ filename = _filename;
        auto fname = msclr::interop::marshal_as<std::string>(filename);
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
        
        // Calculate scaled dimensions maintaining aspect ratio
        auto scaledDimensions = CalculateScaledDimensions(frameWidth, frameHeight, _targetWidth, _targetHeight);
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
        
        // Get encoded packets
        while (ret >= 0) {
            ret = avcodec_receive_packet(_codecContext, _packet);
            if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) {
                break;
            } else if (ret < 0) {
                throw gcnew CodecException("Error receiving packet from encoder");
            }
            
            // Scale packet timestamps
            av_packet_rescale_ts(_packet, _codecContext->time_base, _videoStream->time_base);
            _packet->stream_index = _videoStream->index;
            
            // Write packet
            ret = av_interleaved_write_frame(_formatContext, _packet);
            if (ret < 0) {
                throw gcnew FFMpegException("Error writing packet to file");
            }
            
            av_packet_unref(_packet);
        }
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
            
            auto pkt = av_packet_alloc();
            int ret = 0;
            while (ret >= 0) {
                ret = avcodec_receive_packet(_codecContext, pkt);
                if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) {
                    break;
                }
                if (ret >= 0) {
                    av_packet_rescale_ts(pkt, _codecContext->time_base, _videoStream->time_base);
                    pkt->stream_index = _videoStream->index;
                    av_interleaved_write_frame(_formatContext, pkt);
                    av_packet_unref(pkt);
                }
            }
            av_packet_free(&pkt);
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
            if (!(_formatContext->oformat->flags & AVFMT_NOFILE)) {
                avio_closep(&_formatContext->pb);
            }
            auto tmp = _formatContext;
            avformat_free_context(tmp);
            _formatContext = nullptr;
        }
    }
#pragma endregion

}
