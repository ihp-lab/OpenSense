#pragma once

extern "C" {
#include <libavcodec/avcodec.h>
}

#include "Frame.h"
#include "Packet.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// VideoDecoder decodes video packets into frames using FFmpeg
    /// </summary>
    public ref class VideoDecoder sealed : IDisposable {
    private:
        AVCodecContext* _codecContext;
        AVFrame* _frame;
        bool _flushed;

        void Open(CodecParameters^ parameters);
        void ReceiveFrames(IList<Frame^>^ output);

    public:
        VideoDecoder();

        /// <summary>
        /// Decodes a packet
        /// </summary>
        /// <param name="packet">Packet to decode</param>
        /// <param name="output">List to receive decoded frames, possibly none</param>
        void Decode([NotNull] Packet^ packet, [NotNull] IList<Frame^>^ output);

        /// <summary>
        /// Flushes the decoder and collects the remaining frames
        /// No more packets can be decoded afterwards
        /// </summary>
        /// <param name="output">List to receive remaining frames, possibly none</param>
        void Flush([NotNull] IList<Frame^>^ output);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~VideoDecoder();
        !VideoDecoder();
#pragma endregion
    };
}
