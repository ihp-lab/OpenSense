#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
#include <libavutil/opt.h>
}

#include "Frame.h"
#include "PixelFormat.h"
#include "FFMpegExceptions.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// FileWriter provides video encoding and writing to a mp4 file.
    /// </summary>
    public ref class FileWriter : IDisposable {
    private:
        AVFormatContext* _formatContext;
        AVCodecContext* _codecContext;
        AVStream* _videoStream;
        AVPacket* _packet;
        SwsContext* _swsCtx;
        AVFrame* _convertedFrame;
        
        // Encoder state
        bool _initialized;
        AVRational* _timeBase;
        String^ _filename;
        String^ _encoder;
        PixelFormat _targetFormat;
        int _targetWidth;
        int _targetHeight;
        String^ _additionalArguments;
        long long _lastPts; // Track last PTS for validation

        // Previous frame parameters for SwsContext reuse
        int _prevWidth;
        int _prevHeight;
        AVPixelFormat _prevPixelFormat;

    public:
        /// <summary>
        /// Initialize FileWriter
        /// </summary>
        FileWriter();

        /// <summary>
        /// Gets or sets the output filename for the video file.
        /// Must be set before initializing the encoder.
        /// </summary>
        [NotNull]
        property String^ Filename {
            String^ get() {
                return _filename;
            }
            void set(String^ value) {
                ArgumentNullException::ThrowIfNull(value, "Filename");
                if (value == _filename) {
                    return;
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify filename after encoder initialization");
                }
                _filename = value;
            }
        }

        /// <summary>
        /// Gets or sets the encoder to use for video encoding
        /// Default is "hevc_nvenc".
        /// </summary>
        [NotNull]
        property String^ Encoder {
            String^ get() {
                return _encoder;
            }
            void set(String^ value) {
                ArgumentNullException::ThrowIfNull(value, "Encoder");
                if (value == _encoder) {
                    return;
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify encoder after encoder initialization");
                }
                _encoder = value;
            }
        }

        /// <summary>
        /// Gets or sets the target pixel format for encoded frames.
        /// </summary>
        property PixelFormat TargetFormat {
            PixelFormat get() { 
                return _targetFormat; 
            }
            void set(PixelFormat value) {
                if (!PixelFormatHelper::IsDefined(value)) {
                    throw gcnew ArgumentException("Invalid pixel format");
                }
                if (value == _targetFormat) {
                    return;
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify target format after encoder initialization");
                }
                _targetFormat = value; 
            }
        }

        /// <summary>
        /// Gets or sets the target width for video encoding
        /// Resolution behavior:
        /// - Both TargetWidth and TargetHeight are 0: Use first frame's original dimensions
        /// - One is 0: Scale proportionally based on the non-zero dimension
        /// - Both non-zero: Use specified dimensions
        /// </summary>
        property int TargetWidth {
            int get() {
                return _targetWidth;
            }
            void set(int value) {
                if (value == _targetWidth) {
                    return;
                }
                if (value < 0) {
                    throw gcnew ArgumentException("Target width must be non-negative");
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify target width after encoder initialization");
                }
                _targetWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the target height for video encoding
        /// Resolution behavior:
        /// - Both TargetWidth and TargetHeight are 0: Use first frame's original dimensions
        /// - One is 0: Scale proportionally based on the non-zero dimension
        /// - Both non-zero: Use specified dimensions
        /// </summary>
        property int TargetHeight {
            int get() {
                return _targetHeight;
            }
            void set(int value) {
                if (value == _targetHeight) {
                    return;
                }
                if (value < 0) {
                    throw gcnew ArgumentException("Target height must be non-negative");
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify target height after encoder initialization");
                }
                _targetHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets additional arguments for the encoder
        /// Default is empty string.
        /// </summary>
        [NotNull]
        property String^ AdditionalArguments {
            String^ get() {
                return _additionalArguments;
            }
            void set(String^ value) {
                ArgumentNullException::ThrowIfNull(value, "AdditionalArguments");
                if (value == _additionalArguments) {
                    return;
                }
                if (_initialized) {
                    throw gcnew InvalidOperationException("Cannot modify additional arguments after encoder initialization");
                }
                _additionalArguments = value;
            }
        }

        property PixelFormat Format {
            PixelFormat get() {
                if (!_initialized || _disposed) {
                    return PixelFormat::None;
                }
                return static_cast<PixelFormat>(_codecContext->pix_fmt);
            }
        }

        /// <summary>
        /// Gets the actual encoding width (determined after first frame)
        /// Returns -1 if encoder is not initialized or disposed
        /// </summary>
        property int Width {
            int get() {
                if (!_initialized || _disposed) {
                    return -1;
                }
                return _codecContext->width;
            }
        }

        /// <summary>
        /// Gets the actual encoding height (determined after first frame)
        /// Returns -1 if encoder is not initialized or disposed
        /// </summary>
        property int Height {
            int get() {
                if (!_initialized || _disposed) {
                    return -1;
                }
                return _codecContext->height;
            }
        }

        /// <summary>
        /// Write a frame to the video file
        /// Initializes encoder on first frame with frame parameters
        /// </summary>
        /// <param name="frame">Frame to encode and write</param>
        void WriteFrame([NotNull] Frame^ frame);

    private:
        /// <summary>
        /// Initialize encoder with resolution determined from target settings and first frame dimensions
        /// </summary>
        /// <param name="frameWidth">Width of the first frame</param>
        /// <param name="frameHeight">Height of the first frame</param>
        void InitializeEncoder(PixelFormat frameFormat, int frameWidth, int frameHeight);

        /// <summary>
        /// Encode and write frame to output
        /// </summary>
        void EncodeAndWriteFrame([NotNull] Frame^ frame);
        
        /// <summary>
        /// Receive all available packets from encoder and write them
        /// Uses the internal _packet buffer for receiving
        /// </summary>
        /// <param name="throwOnError">Whether to throw exceptions on errors (false during cleanup)</param>
        void ReceiveAndWritePackets(bool throwOnError);

        /// <summary>
        /// Calculate output dimensions maintaining aspect ratio
        /// </summary>
        /// <param name="sourceWidth">Source frame width</param>
        /// <param name="sourceHeight">Source frame height</param>
        /// <param name="targetWidth">Target width</param>
        /// <param name="targetHeight">Target height</param>
        /// <returns>A tuple containing scaled width and height</returns>
        [returnvalue:TupleElementNames(gcnew array<String^>{"Width", "Height"})]
        static ValueTuple<int, int> CalculateScaledDimensions(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight);

        /// <summary>
        /// Parse additional arguments string into AVDictionary
        /// Supports FFmpeg command line format and dictionary format.
        /// </summary>
        /// <param name="arguments">String containing FFmpeg-style arguments</param>
        /// <returns>Pointer to AVDictionary or nullptr if empty/invalid</returns>
        static AVDictionary* ParseAdditionalArguments(String^ arguments);

#pragma region IDisposable
    private:
        bool _disposed;

        /// <summary>
        /// Throws ObjectDisposedException if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();

    public:
        ~FileWriter();
        !FileWriter();
#pragma endregion
    };
}
