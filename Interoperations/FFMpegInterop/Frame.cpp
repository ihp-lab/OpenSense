#include "pch.h"

#include "Frame.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FFMpegInterop {
    
    Frame::Frame(TimeSpan timestamp, AVFrame* frame)
        : _timestamp(timestamp)
        , _frame(frame)
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
        
        // Initialize lazy data loader
        _lazyData = gcnew Lazy<array<Byte>^>(gcnew Func<array<Byte>^>(this, &Frame::CreateByteArray));
    }

    array<Byte>^ Frame::CreateByteArray() {
        ThrowIfDisposed();
        
        auto format = static_cast<AVPixelFormat>(_frame->format);
        auto isMultiPlane = IsMultiPlaneFormat(format);
        
        if (!isMultiPlane) {
            // Single plane format
            auto srcData = static_cast<byte*>(_frame->data[0]);
            auto srcLength = _frame->linesize[0] * _frame->height;
            auto data = gcnew array<Byte>(srcLength);
            Marshal::Copy(IntPtr(srcData), data, 0, srcLength);
            return data;
        } else {
            // Multi-plane format
            auto srcLength = CalculateMultiPlaneBufferSize(_frame, format);
            auto data = gcnew array<Byte>(srcLength);
            auto offset = 0;
            
            // Y plane
            auto ySize = _frame->linesize[0] * _frame->height;
            Marshal::Copy(IntPtr(_frame->data[0]), data, offset, ySize);
            offset += ySize;
            
            // U plane
            if (!_frame->data[1]) {
                throw gcnew InvalidOperationException("U plane data is null for multi-plane format.");
            }
            auto uHeight = _frame->height / GetVerticalSubsamplingDivisor(format);
            auto uSize = _frame->linesize[1] * uHeight;
            Marshal::Copy(IntPtr(_frame->data[1]), data, offset, uSize);
            offset += uSize;
            
            // V plane
            if (!_frame->data[2]) {
                throw gcnew InvalidOperationException("V plane data is null for multi-plane format.");
            }
            auto vHeight = _frame->height / GetVerticalSubsamplingDivisor(format);
            auto vSize = _frame->linesize[2] * vHeight;
            Marshal::Copy(IntPtr(_frame->data[2]), data, offset, vSize);
            
            return data;
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

    int Frame::CalculateMultiPlaneBufferSize(AVFrame* frame, AVPixelFormat format) {
        auto totalSize = frame->linesize[0] * frame->height; // Y plane

        // U and V planes have the same height calculation
        auto chromaHeight = frame->height / GetVerticalSubsamplingDivisor(format);
        for (int i = 1; i <= 2; i++) {
            if (!frame->data[i]) {
                throw gcnew InvalidOperationException("Chroma plane data is null for multi-plane format.");
            }
            totalSize += frame->linesize[i] * chromaHeight;
        }

        return totalSize;
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