#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
}

#include "MediaType.h"

using namespace System;

namespace FFMpegInterop {
    /// <summary>
    /// Managed wrapper for AVCodecParameters of a stream
    /// Shared by all packets of the stream
    /// </summary>
    public ref class CodecParameters sealed : IDisposable {
    private:
        AVCodecParameters* _parameters;

    internal:
        /// <summary>
        /// Creates a copy of the native parameters
        /// </summary>
        CodecParameters(const AVCodecParameters* source);

        /// <summary>
        /// Gets the internal AVCodecParameters pointer
        /// </summary>
        property const AVCodecParameters* InternalParameters {
            const AVCodecParameters* get();
        }

    public:
        /// <summary>
        /// Gets the media type of the stream
        /// </summary>
        property FFMpegInterop::MediaType MediaType {
            FFMpegInterop::MediaType get();
        }

        /// <summary>
        /// Gets the name of the codec, e.g. "h264"
        /// </summary>
        property String^ CodecName {
            String^ get();
        }

        /// <summary>
        /// Gets the width in pixels, video only
        /// </summary>
        property int Width {
            int get();
        }

        /// <summary>
        /// Gets the height in pixels, video only
        /// </summary>
        property int Height {
            int get();
        }

        /// <summary>
        /// Gets the sample rate in Hz, audio only
        /// </summary>
        property int SampleRate {
            int get();
        }

        /// <summary>
        /// Gets the number of channels, audio only
        /// </summary>
        property int ChannelCount {
            int get();
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~CodecParameters();
        !CodecParameters();
#pragma endregion
    };
}
