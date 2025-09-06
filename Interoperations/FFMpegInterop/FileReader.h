#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswscale/swscale.h>
}

#include "Frame.h"
#include "PixelFormat.h"
#include "FFMpegExceptions.h"

using namespace System;
using namespace System::Collections;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    struct FileReaderUnmanaged;

    /// <summary>
    /// FileReader provides access to video frames from a file using FFmpeg
    /// Implements IEnumerator interface for efficient frame enumeration
    /// </summary>
    public ref class FileReader sealed : public System::Collections::Generic::IEnumerator<Frame^> {
    private:
        AVFormatContext* _formatContext;
        AVCodecContext* _codecContext;
        AVPacket* _packet;
        SwsContext* _swsCtx;
        AVFrame* _rawFrame;
        int _videoStreamIndex;
        AVRational* _timeBase;
        PixelFormat _targetFormat;
        int _targetWidth;
        int _targetHeight;
        int _prevWidth;
        int _prevHeight;
        AVPixelFormat _prevSrcFormat;
        AVPixelFormat _prevDstFormat;

    public:
        /// <summary>
        /// Initialize FileReader with specified video file
        /// </summary>
        /// <param name="filename">Path to video file</param>
        FileReader([NotNull] String^ filename);

        /// <summary>
        /// Gets or sets the target pixel format for decoded frames
        /// </summary>
        property PixelFormat TargetFormat {
            PixelFormat get() { return _targetFormat; }
            void set(PixelFormat value) { 
                if (!PixelFormatHelper::IsSupported(value)) {
                    throw gcnew ArgumentException("Invalid pixel format");
                }
                _targetFormat = value; 
            }
        }

        /// <summary>
        /// Gets or sets the target width for frame scaling (0 = no scaling)
        /// </summary>
        property int TargetWidth {
            int get() { return _targetWidth; }
            void set(int value) { 
                if (value < 0) {
                    throw gcnew ArgumentException("Target width must be non-negative");
                }
                _targetWidth = value; 
            }
        }

        /// <summary>
        /// Gets or sets the target height for frame scaling (0 = no scaling)
        /// </summary>
        property int TargetHeight {
            int get() { return _targetHeight; }
            void set(int value) { 
                if (value < 0) {
                    throw gcnew ArgumentException("Target height must be non-negative");
                }
                _targetHeight = value; 
            }
        }

        /// <summary>
        /// Gets the video stream index
        /// </summary>
        property int VideoStreamIndex {
            int get() { return _videoStreamIndex; }
        }

    private:
        /// <summary>
        /// Internal method to read the next frame from the video stream
        /// </summary>
        Frame^ ReadNextFrame();

        /// <summary>
        /// Calculate output dimensions based on target dimensions and aspect ratio
        /// </summary>
        /// <param name="originalWidth">Original frame width</param>
        /// <param name="originalHeight">Original frame height</param>
        /// <param name="targetWidth">Target width (0 = no scaling)</param>
        /// <param name="targetHeight">Target height (0 = no scaling)</param>
        /// <param name="outputWidth">Reference to store calculated output width</param>
        /// <param name="outputHeight">Reference to store calculated output height</param>
        static void CalculateOutputDimensions(int originalWidth, int originalHeight, int targetWidth, int targetHeight, int& outputWidth, int& outputHeight);

#pragma region IEnumerator<Frame^>
    private:
        // IEnumerator state
        Frame^ _currentFrame;
        bool _endOfFile;
        bool _started;

    public:
        /// <summary>
        /// Gets the current frame in the enumeration
        /// </summary>
        property Frame^ Current {
            virtual Frame^ get() = System::Collections::Generic::IEnumerator<Frame^>::Current::get;
        }

        /// <summary>
        /// Gets the current frame as Object (for non-generic IEnumerator)
        /// </summary>
        property Object^ Current2 {
            virtual Object^ get() = System::Collections::IEnumerator::Current::get;
        }

        /// <summary>
        /// Advances the enumerator to the next frame
        /// </summary>
        /// <returns>true if there is a next frame; false if reached end of file</returns>
        virtual bool MoveNext();

        /// <summary>
        /// Resets the enumerator to the beginning of the file
        /// </summary>
        virtual void Reset();
#pragma endregion

#pragma region IDisposable
    private:
        bool _disposed;

        /// <summary>
        /// Throws ObjectDisposedException if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();

    public:
        ~FileReader();
        !FileReader();
#pragma endregion

    };

}

