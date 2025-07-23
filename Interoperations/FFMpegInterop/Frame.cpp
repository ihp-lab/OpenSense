#include "pch.h"

#include "Frame.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    
    Frame::Frame(AVFrame* frame)
        : _frame(frame)
        , _disposed(false) {
        
        // Validate input parameters
        if (frame == nullptr) {
            throw gcnew ArgumentNullException("frame");
        }
        if (frame->width <= 0) {
            throw gcnew ArgumentException("Frame width must be positive", "frame");
        }
        if (frame->height <= 0) {
            throw gcnew ArgumentException("Frame height must be positive", "frame");
        }
    }

    Frame::Frame(long long pts, int width, int height, PixelFormat format, [NotNull] IntPtr data, int length)
        : _frame(nullptr)
        , _disposed(false) {
        
        // Validate input parameters
        if (data == IntPtr::Zero) {
            throw gcnew ArgumentNullException("data");
        }
        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }
        
        // Validate supported pixel format
        if (!PixelFormatHelper::IsSupported(format)) {
            throw gcnew ArgumentException("Unsupported pixel format", "format");
        }
        auto avFormat = static_cast<AVPixelFormat>(format);
        
        // Create new AVFrame
        _frame = av_frame_alloc();
        if (!_frame) {
            throw gcnew OutOfMemoryException("Failed to allocate AVFrame");
        }
        
        try {
            // Set frame properties
            _frame->width = width;
            _frame->height = height;
            _frame->format = avFormat;
            _frame->pts = pts;
            
            // Allocate buffer for the frame
            auto ret = av_frame_get_buffer(_frame, 0);
            if (ret < 0) {
                throw gcnew InvalidOperationException("Failed to allocate frame buffer");
            }
            
            // Make frame writable
            ret = av_frame_make_writable(_frame);
            if (ret < 0) {
                throw gcnew InvalidOperationException("Failed to make frame writable");
            }
            
            // Copy data from managed array to native frame
            CopyDataToFrame(static_cast<byte*>(data.ToPointer()), length, _frame);
        } catch (...) {
            if (_frame) {
                auto tmp = _frame;
                av_frame_free(&tmp);
                _frame = nullptr;
            }
            throw;
        }
    }

    bool Frame::IsMultiPlaneFormat(AVPixelFormat format) {
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

    int Frame::GetVerticalSubsamplingDivisor(AVPixelFormat format) {
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

    int Frame::CalculateBufferSize(AVFrame* frame, int planeIndex) {
        if (!frame->data[planeIndex]) {
            throw gcnew InvalidOperationException("Chroma plane data is null.");
        }
        auto divisor = 1;
        if (planeIndex != 0) {
            divisor = GetVerticalSubsamplingDivisor(static_cast<AVPixelFormat>(frame->format));
        }
        auto height = frame->height / divisor;
        return frame->linesize[planeIndex] * height;
    }

    int Frame::CalculateBufferSize(AVFrame* frame) {
        auto ySize = CalculateBufferSize(frame, 0);
        if (!IsMultiPlaneFormat(static_cast<AVPixelFormat>(frame->format))) {
            return ySize;
        }
        auto uSize = CalculateBufferSize(frame, 1);
        auto vSize = CalculateBufferSize(frame, 2);
        return ySize + uSize + vSize;
    }

    void Frame::CopyDataToFrame(byte* data, int length, AVFrame* frame) {
        auto format = static_cast<AVPixelFormat>(frame->format);
        auto isMultiPlane = IsMultiPlaneFormat(format);
        
        if (!isMultiPlane) {
            // Single plane format
            auto expectedSize = CalculateBufferSize(frame);
            auto size = frame->linesize[0] * frame->height;
            if (size < expectedSize) {
                throw gcnew ArgumentException("FFMpeg allocated a smaller buffer than expected", "data");
            }
            if (length != expectedSize) {
                throw gcnew ArgumentException("Data buffer size does not match the expected size for the specified format and dimensions", "data");
            }
            Buffer::MemoryCopy(data, frame->data[0], (long long)size, (long long)expectedSize);
        } else {
            // Multi-plane format (YUV422P)
            auto expectedYSize = CalculateBufferSize(frame, 0);
            auto ySize = frame->linesize[0] * frame->height;
            if (ySize < expectedYSize) {
                throw gcnew ArgumentException("FFMpeg allocated a smaller Y plane buffer than expected", "data");
            }
            auto expectedUSize = CalculateBufferSize(frame, 1);
            auto uHeight = frame->height / GetVerticalSubsamplingDivisor(format);
            auto uSize = frame->linesize[1] * uHeight;
            if (uSize < expectedUSize) {
                throw gcnew ArgumentException("FFMpeg allocated a smaller U plane buffer than expected", "data");
            }
            auto expectedVSize = CalculateBufferSize(frame, 2);
            auto vHeight = frame->height / GetVerticalSubsamplingDivisor(format);
            auto vSize = frame->linesize[2] * vHeight;
            if (vSize < expectedVSize) {
                throw gcnew ArgumentException("FFMpeg allocated a smaller V plane buffer than expected", "data");
            }
            if (length != expectedYSize + expectedUSize + expectedVSize) {
                throw gcnew ArgumentException("Data buffer size does not match the expected size for the specified format and dimensions", "data");
            }
            auto offset = 0;
            Buffer::MemoryCopy(data + offset, frame->data[0], (long long)ySize, (long long)expectedYSize);
            offset += ySize;
            Buffer::MemoryCopy(data + offset, frame->data[1], (long long)uSize, (long long)expectedUSize);
            offset += uSize;
            Buffer::MemoryCopy(data + offset, frame->data[2], (long long)vSize, (long long)expectedVSize);
        }
    }

    ValueTuple<IntPtr, int, int> Frame::GetPlaneBuffer(int planeIndex) {
        ThrowIfDisposed();
        
        auto format = static_cast<AVPixelFormat>(_frame->format);
        auto isMultiPlane = IsMultiPlaneFormat(format);
        auto maxPlanes = isMultiPlane ? 3 : 1;
        
        // Validate plane index
        if (planeIndex < 0 || planeIndex >= maxPlanes) {
            throw gcnew ArgumentOutOfRangeException("planeIndex", String::Format("Plane index {0} is out of range. Valid range is 0 to {1}.", planeIndex, maxPlanes - 1));
        }
        
        // Get buffer pointer
        auto bufferPtr = IntPtr(_frame->data[planeIndex]);
        if (bufferPtr == IntPtr::Zero) {
            throw gcnew InvalidOperationException(String::Format("Plane {0} data is null.", planeIndex));
        }
        
        // Get stride
        auto stride = _frame->linesize[planeIndex];
        
        // Calculate length based on plane and format
        auto length = 0;
        if (!isMultiPlane) {
            // Single plane format
            length = CalculateBufferSize(_frame, 0);
        } else {
            // Multi-plane format
            if (planeIndex == 0) {
                // Y plane
                length = stride * _frame->height;
            } else {
                // U or V plane
                auto chromaHeight = _frame->height / GetVerticalSubsamplingDivisor(format);
                length = stride * chromaHeight;
            }
        }
        
        return ValueTuple<IntPtr, int, int>(bufferPtr, stride, length);
    }

#pragma region IDisposable
    void Frame::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("Frame");
        }
    }

    Frame::~Frame() {
        this->!Frame();
        GC::SuppressFinalize(this);
    }

    Frame::!Frame() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_frame) {
            auto tmp = _frame;
            av_frame_free(&tmp);
            _frame = nullptr;
        }
    }
#pragma endregion
} 