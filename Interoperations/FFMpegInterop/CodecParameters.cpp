#include "pch.h"

#include "CodecParameters.h"
#include "FFMpegExceptions.h"

using namespace System;

namespace FFMpegInterop {

    CodecParameters::CodecParameters(const AVCodecParameters* source)
        : _parameters(nullptr)
        , _disposed(false) {
        if (source == nullptr) {
            throw gcnew ArgumentNullException("source");
        }
        _parameters = avcodec_parameters_alloc();
        if (!_parameters) {
            throw gcnew OutOfMemoryException("Could not allocate codec parameters.");
        }
        auto result = avcodec_parameters_copy(_parameters, source);
        if (result < 0) {
            throw gcnew FFMpegException("Could not copy codec parameters: " + FFMpegException::DescribeError(result));
        }
    }

    const AVCodecParameters* CodecParameters::InternalParameters::get() {
        ThrowIfDisposed();
        return _parameters;
    }

    FFMpegInterop::MediaType CodecParameters::MediaType::get() {
        ThrowIfDisposed();
        switch (_parameters->codec_type) {
        case AVMEDIA_TYPE_VIDEO:
            return FFMpegInterop::MediaType::Video;
        case AVMEDIA_TYPE_AUDIO:
            return FFMpegInterop::MediaType::Audio;
        default:
            return FFMpegInterop::MediaType::Unknown;
        }
    }

    String^ CodecParameters::CodecName::get() {
        ThrowIfDisposed();
        return gcnew String(avcodec_get_name(_parameters->codec_id));
    }

    int CodecParameters::Width::get() {
        ThrowIfDisposed();
        return _parameters->codec_type == AVMEDIA_TYPE_VIDEO ? _parameters->width : 0;
    }

    int CodecParameters::Height::get() {
        ThrowIfDisposed();
        return _parameters->codec_type == AVMEDIA_TYPE_VIDEO ? _parameters->height : 0;
    }

    int CodecParameters::SampleRate::get() {
        ThrowIfDisposed();
        return _parameters->codec_type == AVMEDIA_TYPE_AUDIO ? _parameters->sample_rate : 0;
    }

    int CodecParameters::ChannelCount::get() {
        ThrowIfDisposed();
        return _parameters->codec_type == AVMEDIA_TYPE_AUDIO ? _parameters->ch_layout.nb_channels : 0;
    }

#pragma region IDisposable
    void CodecParameters::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("CodecParameters");
        }
    }

    CodecParameters::~CodecParameters() {
        this->!CodecParameters();
        GC::SuppressFinalize(this);
    }

    CodecParameters::!CodecParameters() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_parameters) {
            auto parameters = _parameters;
            avcodec_parameters_free(&parameters);
            _parameters = nullptr;
        }
    }
#pragma endregion
}
