#include "pch.h"

#include "AudioSamples.h"

using namespace System;

namespace FFMpegInterop {

    AudioSamples::AudioSamples(array<Byte>^ data, SampleFormat format, int sampleRate, int channelCount, int sampleCount, long long presentationTimeOffset)
        : _data(data)
        , _format(format)
        , _sampleRate(sampleRate)
        , _channelCount(channelCount)
        , _sampleCount(sampleCount)
        , _presentationTimeOffset(presentationTimeOffset) {
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }
    }
}
