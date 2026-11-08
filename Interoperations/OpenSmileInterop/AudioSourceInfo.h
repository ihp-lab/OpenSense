#pragma once

using namespace System;

namespace OpenSmileInterop {
    /// <summary>
    /// Audio format of a cExternalAudioSource instance, as configured in the configuration file
    /// </summary>
    public ref class AudioSourceInfo sealed {
    private:
        initonly String^ _name;
        initonly int _sampleRate;
        initonly int _channels;
        initonly int _bitsPerSample;
        initonly int _bytesPerSample;
        initonly bool _isFloat;

    internal:
        AudioSourceInfo(String^ name, int sampleRate, int channels, int bitsPerSample, int bytesPerSample, bool isFloat)
            : _name(name), _sampleRate(sampleRate), _channels(channels), _bitsPerSample(bitsPerSample), _bytesPerSample(bytesPerSample), _isFloat(isFloat) {
        }

    public:
        /// <summary>
        /// Instance name in the configuration file
        /// </summary>
        property String^ Name {
            String^ get() { return _name; }
        }

        property int SampleRate {
            int get() { return _sampleRate; }
        }

        property int Channels {
            int get() { return _channels; }
        }

        /// <summary>
        /// Significant bits per sample. 32 for float samples.
        /// </summary>
        property int BitsPerSample {
            int get() { return _bitsPerSample; }
        }

        /// <summary>
        /// Bytes per sample of one channel. May exceed BitsPerSample / 8, e.g. 24-bit samples in 4-byte containers.
        /// </summary>
        property int BytesPerSample {
            int get() { return _bytesPerSample; }
        }

        /// <summary>
        /// True for IEEE float samples, false for integer PCM
        /// </summary>
        property bool IsFloat {
            bool get() { return _isFloat; }
        }
    };
}
