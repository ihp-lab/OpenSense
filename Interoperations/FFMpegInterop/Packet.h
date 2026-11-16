#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
}

#include "CodecParameters.h"

using namespace System;

namespace FFMpegInterop {
    /// <summary>
    /// Managed wrapper for AVPacket, a packet of encoded data of a stream
    /// Packet owns the native AVPacket and handles its lifecycle
    /// </summary>
    public ref class Packet sealed : IDisposable {
    private:
        AVPacket* _packet;
        CodecParameters^ _parameters;

    internal:
        /// <summary>
        /// Takes ownership of the AVPacket pointer
        /// Its timestamps must be in .NET Ticks
        /// </summary>
        Packet(AVPacket* packet, CodecParameters^ parameters);

        /// <summary>
        /// Gets the internal AVPacket pointer
        /// </summary>
        property AVPacket* InternalPacket {
            AVPacket* get();
        }

    public:
        /// <summary>
        /// Gets the codec parameters of the stream this packet belongs to
        /// </summary>
        property CodecParameters^ Parameters {
            CodecParameters^ get();
        }

        /// <summary>
        /// Presentation (display) time offset relative to the start of the file, in .NET Ticks.
        /// </summary>
        property long long PresentationTimeOffset {
            long long get();
        }

        /// <summary>
        /// Decoding time offset relative to the first packet of the same stream, in .NET Ticks.
        /// Consistent with the Psi Envelope's OriginatingTime.
        /// </summary>
        property long long DecodingTimeOffset {
            long long get();
        }

        /// <summary>
        /// Gets the duration in .NET Ticks, zero if unknown
        /// </summary>
        property long long Duration {
            long long get();
        }

        /// <summary>
        /// Gets whether this packet is a key frame
        /// </summary>
        property bool IsKeyFrame {
            bool get();
        }

        /// <summary>
        /// Gets the size of the encoded data in bytes
        /// </summary>
        property int Size {
            int get();
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Packet();
        !Packet();
#pragma endregion
    };
}
