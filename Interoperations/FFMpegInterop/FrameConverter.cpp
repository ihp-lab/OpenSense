#include "pch.h"

extern "C" {
#include <libavutil/imgutils.h>
#include <libavutil/pixdesc.h>
}

#include "FrameConverter.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    FrameConverter::FrameConverter()
        : _context(nullptr)
        , _converted(nullptr)
        , _sourceWidth(0)
        , _sourceHeight(0)
        , _sourceFormat(AV_PIX_FMT_NONE)
        , _sourceColorSpace(0)
        , _sourceFullRange(false)
        , _targetWidth(0)
        , _targetHeight(0)
        , _targetFormat(AV_PIX_FMT_NONE)
        , _disposed(false) {
    }

    void FrameConverter::Convert([NotNull] Frame^ frame, PixelFormat format, int width, int height, IntPtr destination, int stride) {
        ThrowIfDisposed();
        if (frame == nullptr) {
            throw gcnew ArgumentNullException("frame");
        }
        if (destination == IntPtr::Zero) {
            throw gcnew ArgumentNullException("destination");
        }
        if (width <= 0) {
            throw gcnew ArgumentOutOfRangeException("width");
        }
        if (height <= 0) {
            throw gcnew ArgumentOutOfRangeException("height");
        }
        auto targetFormat = static_cast<AVPixelFormat>(format);
        auto targetDescriptor = av_pix_fmt_desc_get(targetFormat);
        if (!targetDescriptor || av_pix_fmt_count_planes(targetFormat) != 1) {
            throw gcnew ArgumentException("The pixel format to write must store all channels in one plane.", "format");
        }
        auto rowLength = av_image_get_linesize(targetFormat, width, 0);
        if (stride < rowLength) {
            throw gcnew ArgumentException("The stride is smaller than one row of pixels.", "stride");
        }

        auto source = frame->InternalFrame;
        auto sourceFormat = static_cast<AVPixelFormat>(source->format);
        auto fullRange = source->color_range == AVCOL_RANGE_JPEG;
        // "J" formats are deprecated, use the regular format with full range instead
        switch (sourceFormat) {
        case AV_PIX_FMT_YUVJ411P:
            sourceFormat = AV_PIX_FMT_YUV411P;
            fullRange = true;
            break;
        case AV_PIX_FMT_YUVJ420P:
            sourceFormat = AV_PIX_FMT_YUV420P;
            fullRange = true;
            break;
        case AV_PIX_FMT_YUVJ422P:
            sourceFormat = AV_PIX_FMT_YUV422P;
            fullRange = true;
            break;
        case AV_PIX_FMT_YUVJ440P:
            sourceFormat = AV_PIX_FMT_YUV440P;
            fullRange = true;
            break;
        case AV_PIX_FMT_YUVJ444P:
            sourceFormat = AV_PIX_FMT_YUV444P;
            fullRange = true;
            break;
        default:
            break;
        }
        auto colorSpace = source->colorspace == AVCOL_SPC_UNSPECIFIED ? SWS_CS_DEFAULT : static_cast<int>(source->colorspace);

        auto unchanged = _context
            && _sourceWidth == source->width
            && _sourceHeight == source->height
            && _sourceFormat == sourceFormat
            && _sourceColorSpace == colorSpace
            && _sourceFullRange == fullRange
            && _targetWidth == width
            && _targetHeight == height
            && _targetFormat == targetFormat;
        if (!unchanged) {
            if (_context) {
                sws_freeContext(_context);
                _context = nullptr;
            }
            if (_converted) {
                auto converted = _converted;
                av_frame_free(&converted);
                _converted = nullptr;
            }
            _converted = av_frame_alloc();
            if (!_converted) {
                throw gcnew OutOfMemoryException("Could not allocate a frame.");
            }
            _converted->format = targetFormat;
            _converted->width = width;
            _converted->height = height;
            auto result = av_frame_get_buffer(_converted, 0);
            if (result < 0) {
                throw gcnew FFMpegException("Could not allocate a picture buffer: " + FFMpegException::DescribeError(result));
            }
            _context = sws_getContext(source->width, source->height, sourceFormat, width, height, targetFormat, SWS_BILINEAR, nullptr, nullptr, nullptr);
            if (!_context) {
                auto sourceName = av_get_pix_fmt_name(static_cast<AVPixelFormat>(source->format));
                throw gcnew FFMpegException(String::Format("Conversion from {0} to {1} is not supported.", gcnew String(sourceName ? sourceName : "an unknown format"), gcnew String(targetDescriptor->name)));
            }

            // libswscale defaults to limited range and BT.601
            int* inverseTable;
            int* table;
            int sourceRange;
            int targetRange;
            int brightness;
            int contrast;
            int saturation;
            if (sws_getColorspaceDetails(_context, &inverseTable, &sourceRange, &table, &targetRange, &brightness, &contrast, &saturation) >= 0) {
                sws_setColorspaceDetails(_context, sws_getCoefficients(colorSpace), fullRange ? 1 : 0, table, targetRange, brightness, contrast, saturation);
            }

            _sourceWidth = source->width;
            _sourceHeight = source->height;
            _sourceFormat = sourceFormat;
            _sourceColorSpace = colorSpace;
            _sourceFullRange = fullRange;
            _targetWidth = width;
            _targetHeight = height;
            _targetFormat = targetFormat;
        }

        // libswscale may write past the end of a row, so convert into an FFmpeg-allocated buffer first
        auto rows = sws_scale(_context, source->data, source->linesize, 0, source->height, _converted->data, _converted->linesize);
        if (rows != height) {
            throw gcnew FFMpegException(String::Format("Conversion wrote {0} rows instead of {1}.", rows, height));
        }
        av_image_copy_plane(static_cast<uint8_t*>(destination.ToPointer()), stride, _converted->data[0], _converted->linesize[0], rowLength, height);
    }

#pragma region IDisposable
    void FrameConverter::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("FrameConverter");
        }
    }

    FrameConverter::~FrameConverter() {
        this->!FrameConverter();
        GC::SuppressFinalize(this);
    }

    FrameConverter::!FrameConverter() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_context) {
            sws_freeContext(_context);
            _context = nullptr;
        }
        if (_converted) {
            auto converted = _converted;
            av_frame_free(&converted);
            _converted = nullptr;
        }
    }
#pragma endregion
}
