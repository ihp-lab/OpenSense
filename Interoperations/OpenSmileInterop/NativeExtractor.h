#pragma once

// This header is shared by managed and native translation units, so it must not include openSMILE headers.
// See NativeExtractor.cpp in OpenSmileInterop.vcxproj for the reason.

#include <cstddef>
#include <cstdint>
#include <memory>
#include <string>
#include <utility>
#include <vector>

#include "NativeExtractorError.h"
#include "NativeTypes.h"

namespace OpenSmileInterop {
    namespace Native {

        /// <summary>
        /// Drives one openSMILE instance on the calling thread. Not thread-safe: calls must be serialized by the caller.
        /// </summary>
        class NativeExtractor {
        public:
            /// <param name="logs">Receives all log entries, including those emitted while the constructor fails. Must outlive this object.</param>
            NativeExtractor(
                const std::wstring& configFilename,
                const std::vector<std::pair<std::wstring, std::wstring>>& options,
                int logLevel,
                std::vector<LogEntry>* logs
            );
            ~NativeExtractor();

            NativeExtractor(const NativeExtractor&) = delete;
            NativeExtractor& operator=(const NativeExtractor&) = delete;

            const std::vector<SourceInfo>& GetSources() const;
            const std::vector<SinkInfo>& GetSinks() const;

            /// <summary>
            /// True once openSMILE has stopped processing by itself (e.g. cTurnDetector with terminateAfterTurns). Further input is discarded.
            /// </summary>
            bool IsTerminated() const;

            /// <summary>
            /// Queues audio for a source and processes as much as possible. Produced frames are appended to the frame buffer.
            /// </summary>
            void Write(int sourceIndex, const std::uint8_t* data, std::size_t length);

            /// <summary>
            /// Signals end of input to all external sources and processes everything that is left. No more input is accepted afterwards.
            /// </summary>
            void Finish();

            /// <summary>
            /// Moves all produced frames into output.
            /// </summary>
            void TakeFrames(std::vector<Frame>& output);

        private:
            struct Impl;
            std::unique_ptr<Impl> _impl;
        };
    }
}
