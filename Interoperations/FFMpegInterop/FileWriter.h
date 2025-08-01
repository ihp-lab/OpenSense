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
    /// FileWriter provides video encoding and writing functionality using FFmpeg NVENC HEVC encoder
    /// Supports variable frame rate MP4 output with real-time encoding
    /// </summary>
    public ref class FileWriter : IDisposable {
    private:
        AVFormatContext* _formatContext;
        AVCodecContext* _codecContext;
        AVStream* _videoStream;
        AVPacket* _packet;
        SwsContext* _swsCtx;
        AVFrame* _convertedFrame;
        String^ _filename;
        
        // Encoder state
        bool _initialized;
        AVRational* _timeBase;
        int _targetWidth;
        int _targetHeight;
        long long _lastPts; // Track last PTS for validation
        
        // Previous frame parameters for SwsContext reuse
        int _prevWidth;
        int _prevHeight;
        AVPixelFormat _prevPixelFormat;

    public:
        /// <summary>
        /// Initialize FileWriter with specified output file and target resolution
        /// Finds and validates NVENC HEVC encoder availability
        /// Resolution behavior:
        /// - Both 0: Use first frame's original dimensions
        /// - One 0: Scale proportionally based on the non-zero dimension
        /// - Both non-zero: Use specified dimensions
        /// </summary>
        /// <param name="filename">Path to output MP4 video file</param>
        /// <param name="targetWidth">Target video width, or 0 for proportional scaling</param>
        /// <param name="targetHeight">Target video height, or 0 for proportional scaling</param>
        FileWriter([NotNull] String^ filename, int targetWidth, int targetHeight);

        /// <summary>
        /// Gets the target width for video encoding
        /// </summary>
        property int TargetWidth {
            int get() {
                return _targetWidth;
            }
        }

        /// <summary>
        /// Gets the target height for video encoding
        /// </summary>
        property int TargetHeight {
            int get() {
                return _targetHeight;
            }
        }

        /// <summary>
        /// Gets the actual encoding width (determined after first frame)
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                return _codecContext->width;
            }
        }

        /// <summary>
        /// Gets the actual encoding height (determined after first frame)
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
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
        void InitializeEncoder(int frameWidth, int frameHeight);
        
        /// <summary>
        /// Calculate output dimensions maintaining aspect ratio
        /// </summary>
        /// <param name="sourceWidth">Source frame width</param>
        /// <param name="sourceHeight">Source frame height</param>
        /// <param name="targetWidth">Target width</param>
        /// <param name="targetHeight">Target height</param>
        /// <returns>A tuple containing scaled width and height</returns>
        [returnvalue: TupleElementNames(gcnew array<String^>{"Width", "Height"})]
        static ValueTuple<int, int> CalculateScaledDimensions(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight);

        /// <summary>
        /// Encode and write frame to output
        /// </summary>
        void EncodeAndWriteFrame([NotNull] Frame^ frame);

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
