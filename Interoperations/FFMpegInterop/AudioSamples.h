#pragma once

#include "SampleFormat.h"

using namespace System;

namespace FFMpegInterop {
    /// <summary>
    /// Decoded audio samples in interleaved format
    /// </summary>
    public ref class AudioSamples sealed {
    private:
        array<Byte>^ _data;
        SampleFormat _format;
        int _sampleRate;
        int _channelCount;
        int _sampleCount;
        long long _presentationTimeOffset;

    internal:
        AudioSamples(array<Byte>^ data, SampleFormat format, int sampleRate, int channelCount, int sampleCount, long long presentationTimeOffset);

    public:
        /// <summary>
        /// Gets the interleaved sample data
        /// </summary>
        property array<Byte>^ Data {
            array<Byte>^ get() { return _data; }
        }

        /// <summary>
        /// Gets the sample format
        /// </summary>
        property SampleFormat Format {
            SampleFormat get() { return _format; }
        }

        /// <summary>
        /// Gets the sample rate in Hz
        /// </summary>
        property int SampleRate {
            int get() { return _sampleRate; }
        }

        /// <summary>
        /// Gets the number of channels
        /// </summary>
        property int ChannelCount {
            int get() { return _channelCount; }
        }

        /// <summary>
        /// Gets the number of samples per channel
        /// </summary>
        property int SampleCount {
            int get() { return _sampleCount; }
        }

        /// <summary>
        /// Presentation time offset of the first sample relative to the start of the file, in .NET Ticks.
        /// </summary>
        property long long PresentationTimeOffset {
            long long get() { return _presentationTimeOffset; }
        }
    };
}
