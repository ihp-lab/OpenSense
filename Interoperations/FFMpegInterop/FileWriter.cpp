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
        , _encoder(String::Empty)
        , _targetFormat(PixelFormat::None)
        , _targetWidth(0)
        , _targetHeight(0)
        , _additionalArguments(String::Empty)
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
        
        if (!frame) {
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
        
        // Find specified encoder
        auto encoder = (String^)_encoder;
        auto encoderName = msclr::interop::marshal_as<std::string>(encoder);
        auto codec = avcodec_find_encoder_by_name(encoderName.c_str());
        if (!codec) {
            String^ errorMessage = "Encoder '" + _encoder + "' not found";
            throw gcnew CodecException(errorMessage);
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

        // Convert filename to std::string
        auto filename = (String^)_filename;
        auto fname = msclr::interop::marshal_as<std::string>(filename);

        // Allocate output format context for MP4
        auto ctx = (AVFormatContext*)nullptr;
        auto ret = avformat_alloc_output_context2(&ctx, nullptr, nullptr, fname.c_str());
        if (ret < 0) {
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
        
        // Open codec with additional options
        AVDictionary* additionalOptions = ParseAdditionalArguments(_additionalArguments);
        ret = avcodec_open2(_codecContext, nullptr, &additionalOptions);
        if (ret < 0) {
            char errbuf[AV_ERROR_MAX_STRING_SIZE] = {0};
            av_strerror(ret, errbuf, sizeof(errbuf));
            String^ errorMessage = gcnew String(errbuf);
            // Clean up dictionary before throwing
            if (additionalOptions) {
                av_dict_free(&additionalOptions);
            }
            throw gcnew CodecException("Could not open codec: " + errorMessage);
        }
        
        // Clean up the dictionary after successful codec opening
        if (additionalOptions) {
            av_dict_free(&additionalOptions);
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
        // Check if frame timebase matches encoder timebase
        auto frameTimeBase = frame->InternalAVFrame->time_base;
        auto encoderTimeBase = _codecContext->time_base;
        if (frameTimeBase.num != encoderTimeBase.num || frameTimeBase.den != encoderTimeBase.den) {
            throw gcnew ArgumentException(
                String::Format("Frame timebase ({0}/{1}) does not match encoder timebase ({2}/{3}).", frameTimeBase.num, frameTimeBase.den, encoderTimeBase.num, encoderTimeBase.den), "frame");
        }

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
            av_frame_copy_props(_convertedFrame, frame->InternalAVFrame);
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
        auto inputPts = frameToEncode->pts;
        
        // Validate PTS for variable frame rate encoding
        if (inputPts == AV_NOPTS_VALUE) {
            throw gcnew ArgumentException("Frame PTS value is invalid. Variable frame rate encoding requires valid timestamps.", "frame");
        }
        
        // For variable frame rate, ensure PTS is monotonically increasing
        if (_lastPts != AV_NOPTS_VALUE && inputPts <= _lastPts) {
            throw gcnew ArgumentException(String::Format("Frame PTS ({0}) must be greater than previous frame PTS ({1}) for variable frame rate encoding.", inputPts, _lastPts), "frame");
        }

        _lastPts = inputPts;
        
        // Encode frame
        auto ret = avcodec_send_frame(_codecContext, frameToEncode);
        if (ret < 0) {
            char error[AV_ERROR_MAX_STRING_SIZE];
            av_make_error_string(error, AV_ERROR_MAX_STRING_SIZE, ret);
            throw gcnew CodecException(String::Format("Error sending frame to encoder: {0}", gcnew String(error)));
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

    AVDictionary* FileWriter::ParseAdditionalArguments(String^ arguments) {
        if (String::IsNullOrWhiteSpace(arguments)) {
            return nullptr;
        }

        AVDictionary* dict = nullptr;
        
        try {
            // Convert managed string to std::string
            auto argsStr = msclr::interop::marshal_as<std::string>(arguments);
            
            // Trim leading/trailing whitespace
            auto start = argsStr.find_first_not_of(" \t");
            if (start == std::string::npos) {
                return nullptr;
            }
            auto end = argsStr.find_last_not_of(" \t");
            argsStr = argsStr.substr(start, end - start + 1);
            
            // Check if it's FFmpeg command line format (starts with -)
            if (argsStr[0] == '-') {
                // Parse FFmpeg command line format: "-preset slow -rc vbr_hq -spatial_aq 1"
                std::string::size_type pos = 0;
                while (pos < argsStr.length()) {
                    // Find next argument starting with -
                    auto argStart = argsStr.find('-', pos);
                    if (argStart == std::string::npos) break;
                    
                    // Skip the - character
                    argStart++;
                    
                    // Find the end of parameter name (next space)
                    auto nameEnd = argsStr.find(' ', argStart);
                    if (nameEnd == std::string::npos) break;
                    
                    // Extract parameter name
                    auto paramName = argsStr.substr(argStart, nameEnd - argStart);
                    
                    // Find the start of value (skip spaces)
                    auto valueStart = argsStr.find_first_not_of(' ', nameEnd);
                    if (valueStart == std::string::npos) break;
                    
                    // Find the end of value (next - or end of string)
                    auto valueEnd = argsStr.find(" -", valueStart);
                    if (valueEnd == std::string::npos) {
                        valueEnd = argsStr.length();
                    }
                    
                    // Extract parameter value and trim
                    auto paramValue = argsStr.substr(valueStart, valueEnd - valueStart);
                    auto valueEndTrim = paramValue.find_last_not_of(' ');
                    if (valueEndTrim != std::string::npos) {
                        paramValue = paramValue.substr(0, valueEndTrim + 1);
                    }
                    
                    // Add to dictionary
                    av_dict_set(&dict, paramName.c_str(), paramValue.c_str(), 0);
                    
                    pos = valueEnd;
                }
            } else {
                // Try dictionary format first: "preset=slow rc=vbr_hq spatial_aq=1"
                auto ret = av_dict_parse_string(&dict, argsStr.c_str(), "=", " ", 0);
                if (ret < 0) {
                    // If space-separated format fails, try colon-separated format
                    if (dict) {
                        av_dict_free(&dict);
                        dict = nullptr;
                    }
                    ret = av_dict_parse_string(&dict, argsStr.c_str(), "=", ":", 0);
                    if (ret < 0) {
                        // If both formats fail, clean up and return nullptr
                        if (dict) {
                            av_dict_free(&dict);
                        }
                        return nullptr;
                    }
                }
            }
        }
        catch (...) {
            // Clean up on any exception
            if (dict) {
                av_dict_free(&dict);
            }
            return nullptr;
        }

        return dict;
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
