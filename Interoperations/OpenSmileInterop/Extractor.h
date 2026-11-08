#pragma once

#include "AudioSourceInfo.h"
#include "Frame.h"
#include "LogHandler.h"
#include "NativeExtractor.h"
#include "SinkInfo.h"

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics::CodeAnalysis;

namespace OpenSmileInterop {
    /// <summary>
    /// Runs an openSMILE configuration on audio written by the caller.
    /// Audio enters through cExternalAudioSource instances, and features leave through cExternalSink instances.
    /// All processing happens synchronously on the calling thread.
    ///
    /// Thread safety: an instance is not thread-safe; calls must be serialized by the caller.
    /// Different instances may run concurrently.
    /// </summary>
    public ref class Extractor sealed : IDisposable {
    private:
        Native::NativeExtractor* _native;
        std::vector<Native::LogEntry>* _logs;
        LogHandler^ _logHandler;
        IReadOnlyList<AudioSourceInfo^>^ _sources;
        IReadOnlyList<SinkInfo^>^ _sinks;

    public:
        /// <summary>
        /// Loads the configuration and creates all component instances
        /// </summary>
        /// <param name="configFilename">Configuration file. Must be representable in the ANSI code page, because openSMILE opens files with narrow APIs.</param>
        /// <param name="options">Values for options declared with \cm[...] in the configuration, like SMILExtract command line options without the leading '-'</param>
        /// <param name="logLevel">openSMILE verbosity for messages and warnings. Errors are always reported.</param>
        /// <param name="logHandler">Receives log messages synchronously, after each call into openSMILE. May be null.</param>
        Extractor(
            [NotNull] String^ configFilename,
            IEnumerable<KeyValuePair<String^, String^>>^ options,
            int logLevel,
            LogHandler^ logHandler
        );

        /// <summary>
        /// cExternalAudioSource instances, in configuration order. Write takes an index into this list.
        /// </summary>
        property IReadOnlyList<AudioSourceInfo^>^ Sources {
            IReadOnlyList<AudioSourceInfo^>^ get();
        }

        /// <summary>
        /// cExternalSink instances, in configuration order. Frame.SinkIndex is an index into this list.
        /// </summary>
        property IReadOnlyList<SinkInfo^>^ Sinks {
            IReadOnlyList<SinkInfo^>^ get();
        }

        /// <summary>
        /// True once openSMILE has stopped processing by itself, e.g. cTurnDetector with terminateAfterTurns.
        /// Further input is discarded, and Finish produces no more frames.
        /// </summary>
        property bool IsTerminated {
            bool get();
        }

        /// <summary>
        /// Writes interleaved audio in the format of the source and processes as much as possible.
        /// Produced frames (0 or more) are appended to output.
        /// </summary>
        /// <param name="count">Must be a multiple of Channels * BytesPerSample of the source</param>
        void Write(int sourceIndex, [NotNull] cli::array<Byte>^ data, int offset, int count, [NotNull] IList<Frame^>^ output);

        /// <summary>
        /// Writes interleaved audio in the format of the source and processes as much as possible.
        /// Produced frames (0 or more) are appended to output.
        /// </summary>
        void Write(int sourceIndex, [NotNull] cli::array<Byte>^ data, [NotNull] IList<Frame^>^ output);

        /// <summary>
        /// Signals end of input and processes everything that is left, e.g. functionals over the whole input.
        /// Produced frames are appended to output. Write and Finish cannot be called afterwards.
        /// </summary>
        void Finish([NotNull] IList<Frame^>^ output);

    private:
        /// <summary>
        /// Delivers log entries collected by the native side. Called after every native call, including failed ones.
        /// </summary>
        void FlushLogs();

        void AppendFrames(IList<Frame^>^ output);

        static Exception^ ConvertError(const Native::ExtractorError& error);

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Extractor();
        !Extractor();
#pragma endregion
    };
}
