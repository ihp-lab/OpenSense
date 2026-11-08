#include "pch.h"

// msclr headers include windows.h, whose IServiceProvider clashes with System::IServiceProvider once
// `using namespace System` (pulled in by the headers below) is in effect.
#include <cstring>
#include <msclr/marshal_cppstd.h>

#include "Extractor.h"
#include "OpenSmileException.h"

using namespace System;
using namespace System::Collections::Generic;

namespace OpenSmileInterop {
    Extractor::Extractor(
        [NotNull] String^ configFilename,
        IEnumerable<KeyValuePair<String^, String^>>^ options,
        int logLevel,
        LogHandler^ logHandler
    )
        : _native(nullptr)
        , _logs(nullptr)
        , _logHandler(logHandler)
        , _disposed(false) {

        ArgumentNullException::ThrowIfNull(configFilename, "configFilename");

        std::vector<std::pair<std::wstring, std::wstring>> nativeOptions;
        if (options != nullptr) {
            for each (auto option in options) {
                ArgumentNullException::ThrowIfNull(option.Key, "options");
                auto value = option.Value != nullptr ? option.Value : String::Empty;
                nativeOptions.emplace_back(msclr::interop::marshal_as<std::wstring>(option.Key), msclr::interop::marshal_as<std::wstring>(value));
            }
        }

        _logs = new std::vector<Native::LogEntry>();
        Exception^ exception = nullptr;
        try {
            _native = new Native::NativeExtractor(msclr::interop::marshal_as<std::wstring>(configFilename), nativeOptions, logLevel, _logs);
        } catch (const Native::ExtractorError& error) {
            exception = ConvertError(error);
        }
        // Logs explain configuration errors, so they are delivered before throwing.
        FlushLogs();
        if (exception != nullptr) {
            throw exception;
        }

        auto sources = gcnew List<AudioSourceInfo^>();
        for (const auto& source : _native->GetSources()) {
            sources->Add(gcnew AudioSourceInfo(
                gcnew String(source.name.c_str()),
                source.sampleRate,
                source.channels,
                source.bitsPerSample,
                source.bytesPerSample,
                source.isFloat
            ));
        }
        _sources = sources->AsReadOnly();

        auto sinks = gcnew List<SinkInfo^>();
        for (const auto& sink : _native->GetSinks()) {
            auto elementNames = gcnew List<String^>(static_cast<int>(sink.elementNames.size()));
            for (const auto& elementName : sink.elementNames) {
                elementNames->Add(gcnew String(elementName.c_str()));
            }
            sinks->Add(gcnew SinkInfo(gcnew String(sink.name.c_str()), elementNames->AsReadOnly()));
        }
        _sinks = sinks->AsReadOnly();
    }

    IReadOnlyList<AudioSourceInfo^>^ Extractor::Sources::get() {
        ThrowIfDisposed();
        return _sources;
    }

    IReadOnlyList<SinkInfo^>^ Extractor::Sinks::get() {
        ThrowIfDisposed();
        return _sinks;
    }

    bool Extractor::IsTerminated::get() {
        ThrowIfDisposed();
        return _native->IsTerminated();
    }

    void Extractor::Write(int sourceIndex, [NotNull] cli::array<Byte>^ data, int offset, int count, [NotNull] IList<Frame^>^ output) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(data, "data");
        ArgumentNullException::ThrowIfNull(output, "output");
        if (offset < 0 || count < 0 || offset > data->Length - count) {
            throw gcnew ArgumentOutOfRangeException("count", "offset and count must describe a range within data.");
        }

        pin_ptr<Byte> pinned = nullptr;
        if (count > 0) {
            pinned = &data[offset];
        }
        Exception^ exception = nullptr;
        try {
            _native->Write(sourceIndex, pinned, static_cast<std::size_t>(count));
        } catch (const Native::ExtractorError& error) {
            exception = ConvertError(error);
        }
        pinned = nullptr;
        FlushLogs();
        if (exception != nullptr) {
            throw exception;
        }
        AppendFrames(output);
    }

    void Extractor::Write(int sourceIndex, [NotNull] cli::array<Byte>^ data, [NotNull] IList<Frame^>^ output) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(data, "data");
        Write(sourceIndex, data, 0, data->Length, output);
    }

    void Extractor::Finish([NotNull] IList<Frame^>^ output) {
        ThrowIfDisposed();
        ArgumentNullException::ThrowIfNull(output, "output");

        Exception^ exception = nullptr;
        try {
            _native->Finish();
        } catch (const Native::ExtractorError& error) {
            exception = ConvertError(error);
        }
        FlushLogs();
        if (exception != nullptr) {
            throw exception;
        }
        AppendFrames(output);
    }

    void Extractor::FlushLogs() {
        if (_logs == nullptr || _logs->empty()) {
            return;
        }
        auto messages = gcnew List<LogMessage^>(static_cast<int>(_logs->size()));
        for (const auto& entry : *_logs) {
            messages->Add(gcnew LogMessage(
                static_cast<LogType>(entry.type),
                entry.level,
                gcnew String(entry.module.c_str()),
                gcnew String(entry.text.c_str())
            ));
        }
        // Cleared before invoking the handler, so that a throwing handler does not deliver the same messages twice.
        _logs->clear();
        if (_logHandler == nullptr) {
            return;
        }
        for each (auto message in messages) {
            _logHandler(message);
        }
    }

    void Extractor::AppendFrames(IList<Frame^>^ output) {
        std::vector<Native::Frame> frames;
        _native->TakeFrames(frames);
        for (const auto& frame : frames) {
            auto values = gcnew cli::array<float>(static_cast<int>(frame.values.size()));
            if (values->Length > 0) {
                pin_ptr<float> destination = &values[0];
                std::memcpy(destination, frame.values.data(), values->Length * sizeof(float));
            }
            output->Add(gcnew Frame(frame.sinkIndex, frame.index, frame.time, frame.period, frame.length, values));
        }
    }

    Exception^ Extractor::ConvertError(const Native::ExtractorError& error) {
        auto message = gcnew String(error.message.c_str());
        switch (error.kind) {
        case Native::ErrorKind::InvalidArgument:
            return gcnew ArgumentException(message);
        case Native::ErrorKind::InvalidOperation:
            return gcnew InvalidOperationException(message);
        default:
            return gcnew OpenSmileException(message);
        }
    }

#pragma region IDisposable
    void Extractor::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Extractor::~Extractor() {
        if (_disposed) {
            return;
        }

        delete _native;
        _native = nullptr;
        // Only a deterministic dispose delivers logs; a finalizer must not call into user code.
        FlushLogs();
        this->!Extractor();
        _disposed = true;
    }

    Extractor::!Extractor() {
        // Also runs for an instance whose constructor threw, so every member may still be null.
        delete _native;
        _native = nullptr;
        delete _logs;
        _logs = nullptr;
    }
#pragma endregion
}
