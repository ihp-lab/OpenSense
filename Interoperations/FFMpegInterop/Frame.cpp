#include "pch.h"

extern "C" {
#include <libavutil/pixdesc.h>
}

#include "Frame.h"

using namespace System;

namespace FFMpegInterop {

    Frame::Frame(AVFrame* frame)
        : _frame(frame)
        , _disposed(false) {
        if (frame == nullptr) {
            throw gcnew ArgumentNullException("frame");
        }
        if (frame->width <= 0 || frame->height <= 0) {
            throw gcnew ArgumentException("The frame has no picture.", "frame");
        }
    }

    AVFrame* Frame::InternalFrame::get() {
        ThrowIfDisposed();
        return _frame;
    }

    long long Frame::PresentationTimeOffset::get() {
        ThrowIfDisposed();
        return _frame->pts;
    }

    int Frame::Width::get() {
        ThrowIfDisposed();
        return _frame->width;
    }

    int Frame::Height::get() {
        ThrowIfDisposed();
        return _frame->height;
    }

    PixelFormat Frame::Format::get() {
        ThrowIfDisposed();
        return static_cast<PixelFormat>(_frame->format);
    }

    FrameFlags Frame::Flags::get() {
        ThrowIfDisposed();
        return static_cast<FrameFlags>(_frame->flags);
    }

    int Frame::PlaneCount::get() {
        ThrowIfDisposed();
        auto count = av_pix_fmt_count_planes(static_cast<AVPixelFormat>(_frame->format));
        return count < 0 ? 0 : count;
    }

    ValueTuple<IntPtr, int, int> Frame::GetPlaneBuffer(int planeIndex) {
        ThrowIfDisposed();

        auto format = static_cast<AVPixelFormat>(_frame->format);
        auto count = av_pix_fmt_count_planes(format);
        if (planeIndex < 0 || planeIndex >= count) {
            throw gcnew ArgumentOutOfRangeException("planeIndex", String::Format("Plane index {0} is out of range. The frame has {1} planes.", planeIndex, count));
        }
        if (_frame->data[planeIndex] == nullptr) {
            throw gcnew InvalidOperationException(String::Format("Plane {0} is not in main memory. Frames of hardware decoders must be downloaded first.", planeIndex));
        }

        // Only chroma planes (1 and 2) are subsampled vertically
        auto height = _frame->height;
        if (planeIndex == 1 || planeIndex == 2) {
            auto descriptor = av_pix_fmt_desc_get(format);
            height = AV_CEIL_RSHIFT(height, descriptor->log2_chroma_h);
        }
        auto stride = _frame->linesize[planeIndex];
        return ValueTuple<IntPtr, int, int>(IntPtr(_frame->data[planeIndex]), stride, stride * height);
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
            auto frame = _frame;
            av_frame_free(&frame);
            _frame = nullptr;
        }
    }
#pragma endregion
}
