#pragma once

using namespace System;

namespace OpenSmileInterop {
    /// <summary>
    /// A feature vector produced by a cExternalSink instance
    /// </summary>
    public ref class Frame sealed {
    private:
        initonly int _sinkIndex;
        initonly long long _index;
        initonly double _time;
        initonly double _period;
        initonly double _length;
        initonly cli::array<float>^ _values;

    internal:
        Frame(int sinkIndex, long long index, double time, double period, double length, cli::array<float>^ values)
            : _sinkIndex(sinkIndex), _index(index), _time(time), _period(period), _length(length), _values(values) {
        }

    public:
        /// <summary>
        /// Index into Extractor.Sinks
        /// </summary>
        property int SinkIndex {
            int get() { return _sinkIndex; }
        }

        /// <summary>
        /// Index of the frame in the data memory level read by the sink
        /// </summary>
        property long long Index {
            long long get() { return _index; }
        }

        /// <summary>
        /// Start of the frame in seconds, counted from the first sample written to the sources
        /// </summary>
        property double Time {
            double get() { return _time; }
        }

        /// <summary>
        /// Frame period of the level in seconds. 0 for levels with variable frame lengths, e.g. turn-based functionals.
        /// </summary>
        property double Period {
            double get() { return _period; }
        }

        /// <summary>
        /// Length of the frame in seconds
        /// </summary>
        property double Length {
            double get() { return _length; }
        }

        property cli::array<float>^ Values {
            cli::array<float>^ get() { return _values; }
        }
    };
}
