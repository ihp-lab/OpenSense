#pragma once

// Shared by managed and native translation units, so it must not include openSMILE headers.
// See NativeExtractor.cpp in OpenSmileInterop.vcxproj for the reason.

#include <string>
#include <vector>

namespace OpenSmileInterop {
    namespace Native {

        struct LogEntry {
            int type;
            int level;
            std::wstring module;
            std::wstring text;
        };

        struct SourceInfo {
            std::wstring name;
            int sampleRate;
            int channels;
            int bitsPerSample;
            int bytesPerSample;
            bool isFloat;
        };

        struct SinkInfo {
            std::wstring name;
            std::vector<std::wstring> elementNames;
        };

        struct Frame {
            int sinkIndex;
            long long index;
            double time;
            double period;
            double length;
            std::vector<float> values;
        };
    }
}
