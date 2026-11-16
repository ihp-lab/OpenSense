#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
}

#include "AudioSamples.h"
#include "Packet.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// AudioDecoder decodes audio packets into interleaved samples using FFmpeg
    /// </summary>
    public ref class AudioDecoder sealed : IDisposable {
    private:
        AVCodecContext* _codecContext;
        AVFrame* _frame;
        bool _flushed;

        // In .NET ticks. AV_NOPTS_VALUE until known.
        long long _nextPresentationTime;

        void Open(CodecParameters^ parameters);
        void ReceiveSamples(IList<AudioSamples^>^ output);

    public:
        AudioDecoder();

        /// <summary>
        /// Decodes a packet
        /// </summary>
        /// <param name="packet">Packet to decode</param>
        /// <param name="output">List to receive decoded samples, possibly none</param>
        void Decode([NotNull] Packet^ packet, [NotNull] IList<AudioSamples^>^ output);

        /// <summary>
        /// Flushes the decoder and collects the remaining samples
        /// No more packets can be decoded afterwards
        /// </summary>
        /// <param name="output">List to receive remaining samples, possibly none</param>
        void Flush([NotNull] IList<AudioSamples^>^ output);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~AudioDecoder();
        !AudioDecoder();
#pragma endregion
    };
}
