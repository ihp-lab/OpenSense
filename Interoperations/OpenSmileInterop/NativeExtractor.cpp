// Compiled without /clr. This is the only translation unit that includes openSMILE headers.

#include "NativeExtractor.h"

// Standard headers must come before openSMILE headers: smileUtil.h redefines the inline keyword,
// which breaks standard headers included after it.
#include <algorithm>
#include <clocale>
#include <cstdlib>
#include <limits>
#include <mutex>

#include <core/smileCommon.hpp>
#include <core/smileLogger.hpp>
#include <core/exceptions.hpp>
#include <core/commandlineParser.hpp>
#include <core/configManager.hpp>
#include <core/componentManager.hpp>
#include <iocore/externalAudioSource.hpp>
#include <iocore/externalSource.hpp>
#include <iocore/externalSink.hpp>

#ifdef inline
#undef inline
#endif

namespace OpenSmileInterop {
    namespace Native {

        namespace {
            // openSMILE logs the text of every exception it throws and then discards it (cSMILException::getText() returns NULL afterwards).
            // Errors are therefore never filtered, so that exceptions can be described by the errors logged during the call.
            constexpr int ERROR_LOG_LEVEL = 100;

            // A tick loop that keeps running this long is not settling; fail instead of hanging the caller.
            constexpr long long MAX_TICKS_PER_PUMP = 100000000;

            // writeData takes the byte count as int.
            constexpr std::size_t MAX_CHUNK_BYTES = 1 << 30;

            // cComponentManager registers component types into process-wide statics while being created and destroyed.
            std::mutex s_lifecycleMutex;

            std::string ToAnsi(const std::wstring& text, const wchar_t* description) {
                if (text.empty()) {
                    return std::string();
                }
                // openSMILE opens files with narrow fopen, so text must survive the round trip through the ANSI code page.
                // WC_NO_BEST_FIT_CHARS and the default char flag are not allowed when the ANSI code page is UTF-8.
                auto isUtf8 = GetACP() == CP_UTF8;
                auto flags = isUtf8 ? 0 : WC_NO_BEST_FIT_CHARS;
                BOOL usedDefaultChar = FALSE;
                auto usedDefaultCharPointer = isUtf8 ? nullptr : &usedDefaultChar;
                auto size = WideCharToMultiByte(CP_ACP, flags, text.c_str(), static_cast<int>(text.size()), nullptr, 0, nullptr, usedDefaultCharPointer);
                if (size <= 0 || usedDefaultChar) {
                    throw ExtractorError(ErrorKind::InvalidArgument, std::wstring(description) + L" cannot be represented in the ANSI code page: " + text);
                }
                std::string result(size, '\0');
                WideCharToMultiByte(CP_ACP, flags, text.c_str(), static_cast<int>(text.size()), &result[0], size, nullptr, nullptr);
                return result;
            }

            std::wstring FromAnsi(const char* text) {
                if (text == nullptr || *text == '\0') {
                    return std::wstring();
                }
                auto size = MultiByteToWideChar(CP_ACP, 0, text, -1, nullptr, 0);
                if (size <= 1) {
                    return std::wstring();
                }
                std::wstring result(size, L'\0');
                MultiByteToWideChar(CP_ACP, 0, text, -1, &result[0], size);
                result.resize(size - 1);
                return result;
            }

            /// <summary>
            /// Binds per-thread state that openSMILE expects, and restores it on exit, because pipeline calls arrive on arbitrary pool threads.
            /// - The logger is thread-local in openSMILE.
            /// - Config parsing uses locale-dependent number conversion. The locale is switched per thread only,
            ///   unlike smileCommon_fixLocaleEnUs, which would change it for every native library in the process.
            /// </summary>
            class ThreadStateScope {
            public:
                explicit ThreadStateScope(cSmileLogger* logger)
                    : _previousLogger(SMILE_LOG_GLOBAL)
                    , _previousLocaleMode(_configthreadlocale(_ENABLE_PER_THREAD_LOCALE)) {
                    auto numericLocale = std::setlocale(LC_NUMERIC, nullptr);
                    _previousNumericLocale = numericLocale != nullptr ? numericLocale : "C";
                    std::setlocale(LC_NUMERIC, "C");
                    logger->useForCurrentThread();
                }

                ~ThreadStateScope() {
                    SMILE_LOG_GLOBAL = _previousLogger;
                    std::setlocale(LC_NUMERIC, _previousNumericLocale.c_str());
                    _configthreadlocale(_previousLocaleMode);
                }

                ThreadStateScope(const ThreadStateScope&) = delete;
                ThreadStateScope& operator=(const ThreadStateScope&) = delete;

            private:
                cSmileLogger* _previousLogger;
                int _previousLocaleMode;
                std::string _previousNumericLocale;
            };

            /// <summary>
            /// cComponentManager::runSingleThreaded sets the priority of the calling thread, which belongs to the pipeline's thread pool.
            /// </summary>
            class ThreadPriorityScope {
            public:
                ThreadPriorityScope()
                    : _thread(GetCurrentThread())
                    , _previousPriority(GetThreadPriority(GetCurrentThread())) {
                }

                ~ThreadPriorityScope() {
                    SetThreadPriority(_thread, _previousPriority);
                }

                ThreadPriorityScope(const ThreadPriorityScope&) = delete;
                ThreadPriorityScope& operator=(const ThreadPriorityScope&) = delete;

            private:
                HANDLE _thread;
                int _previousPriority;
            };

            enum class State {
                Running,
                Terminated,
                Finished,
                Faulted,
            };

            enum class WriteResult {
                Written,
                Shrunk,
                Blocked,
            };

            /// <summary>
            /// Audio waiting to be written into a source's data memory level.
            /// Writes fail while the level is full, e.g. when processing waits for another source.
            /// </summary>
            struct PendingInput {
                std::size_t frameBytes = 0;
                std::size_t maxChunk = 0;
                std::size_t chunkLimit = 0;
                std::vector<std::uint8_t> buffer;
                std::size_t offset = 0;

                std::size_t Remaining() const {
                    return buffer.size() - offset;
                }

                const std::uint8_t* Current() const {
                    return buffer.data() + offset;
                }

                void Append(const std::uint8_t* data, std::size_t length) {
                    // Compact lazily, so that steady streaming does not shift the buffer on every write.
                    if (offset > 0 && offset >= buffer.size() / 2) {
                        buffer.erase(buffer.begin(), buffer.begin() + offset);
                        offset = 0;
                    }
                    buffer.insert(buffer.end(), data, data + length);
                }

                void Consume(std::size_t length) {
                    offset += length;
                    if (offset == buffer.size()) {
                        buffer.clear();
                        offset = 0;
                    }
                }

                void Clear() {
                    buffer.clear();
                    offset = 0;
                }
            };
        }

        struct NativeExtractor::Impl {
            struct SinkContext {
                Impl* owner;
                int sinkIndex;
            };

            // Members are destroyed in reverse declaration order, so the logger outlives the components that log through it.
            std::vector<LogEntry>* _logs;
            std::unique_ptr<cSmileLogger> _logger;
            std::string _configFilename;
            std::vector<std::string> _arguments;
            std::vector<const char*> _argumentPointers;
            std::unique_ptr<cCommandlineParser> _commandLine;
            std::unique_ptr<cConfigManager> _configManager;
            std::unique_ptr<cComponentManager> _componentManager;

            std::vector<cExternalAudioSource*> _audioSources;
            std::vector<cExternalSource*> _unsupportedSources;
            std::vector<cExternalSink*> _sinks;
            std::vector<std::unique_ptr<SinkContext>> _sinkContexts;
            std::vector<SourceInfo> _sourceInfos;
            std::vector<SinkInfo> _sinkInfos;
            std::vector<PendingInput> _pendingInputs;
            std::vector<Frame> _frames;

            long long _tickNumber = 0;
            State _currentState = State::Running;
            std::wstring _sinkError;

            Impl(const std::wstring& configFilename, const std::vector<std::pair<std::wstring, std::wstring>>& options, int logLevel, std::vector<LogEntry>* logs)
                : _logs(logs) {
                // No console and no log file: everything goes through the callback to the caller.
                _logger.reset(new cSmileLogger(logLevel, nullptr, 0, 0));
                _logger->setColoredOutput(false);
                _logger->setLogLevel(LOG_ERROR, ERROR_LOG_LEVEL);
                // Debug output is compiled into the Debug build of openSMILE only, and some of it crashes on wrong format strings
                // (e.g. '%s' for a double in cSpecResample). SMILEapi disables it the same way.
                _logger->setLogLevel(LOG_DEBUG, 0);
                _logger->setCallback([logs](int type, int level, const char* text, const char* module) {
                    logs->push_back(LogEntry{ type, level, FromAnsi(module), FromAnsi(text) });
                });

                std::lock_guard<std::mutex> lock(s_lifecycleMutex);
                ThreadStateScope scope(_logger.get());
                try {
                    Execute([&]() {
                        Initialize(configFilename, options);
                    });
                } catch (...) {
                    ReleaseComponents();
                    throw;
                }
            }

            ~Impl() {
                std::lock_guard<std::mutex> lock(s_lifecycleMutex);
                ThreadStateScope scope(_logger.get());
                try {
                    ReleaseComponents();
                } catch (...) {
                    // A destructor must not throw; openSMILE has already logged the cause.
                }
            }

#pragma region Initialization
            void Initialize(const std::wstring& configFilename, const std::vector<std::pair<std::wstring, std::wstring>>& options) {
                _configFilename = ToAnsi(configFilename, L"Configuration filename");

                // Options are passed to openSMILE the same way as SMILExtract command line arguments: -name value
                _arguments.push_back("SMILExtract");
                for (const auto& option : options) {
                    if (option.first.empty()) {
                        throw ExtractorError(ErrorKind::InvalidArgument, L"Option names must not be empty.");
                    }
                    if (option.first == L"h") {
                        // The command line parser treats -h as a request for usage information and stops parsing.
                        throw ExtractorError(ErrorKind::InvalidArgument, L"Option '-h' is reserved by openSMILE.");
                    }
                    _arguments.push_back("-" + ToAnsi(option.first, L"Option name"));
                    if (!option.second.empty()) {
                        _arguments.push_back(ToAnsi(option.second, L"Option value"));
                    }
                }
                for (const auto& argument : _arguments) {
                    _argumentPointers.push_back(argument.c_str());
                }

                // Same sequence as smile_initialize in SMILEapi.cpp
                _commandLine.reset(new cCommandlineParser(static_cast<int>(_argumentPointers.size()), _argumentPointers.data()));
                _commandLine->parse();
                _configManager.reset(new cConfigManager(_commandLine.get()));
                _componentManager.reset(new cComponentManager(_configManager.get(), componentlist));
                _configManager->addReader(new cFileConfigReader(_configFilename.c_str(), -1, _commandLine.get()));
                _configManager->readConfig();
                // Parse again to pick up the options declared by \cm[...] in the configuration
                _commandLine->parse(true, false);
                _componentManager->createInstances(0);
                if (!_componentManager->isReady()) {
                    throw ExtractorError(ErrorKind::OpenSmile, L"openSMILE did not become ready. The configuration may not declare any component instance.");
                }

                // The parser silently ignores unknown options, which would hide typos.
                for (const auto& option : options) {
                    if (!_commandLine->optionExists(ToAnsi(option.first, L"Option name").c_str())) {
                        throw ExtractorError(ErrorKind::InvalidArgument, L"Unknown option '" + option.first + L"'. Options must be declared in the configuration with \\cm[...].");
                    }
                }

                CollectComponents();
            }

            void CollectComponents() {
                for (auto i = 0; _componentManager->getComponentInstanceType(i) != nullptr; i++) {
                    auto component = _componentManager->getComponentInstance(i);
                    if (component == nullptr) {
                        continue;
                    }

                    if (auto audioSource = dynamic_cast<cExternalAudioSource*>(component)) {
                        auto isFloat = audioSource->getNBits() == 33; // openSMILE encodes 32-bit float samples as nBits = 33
                        _sourceInfos.push_back(SourceInfo{
                            FromAnsi(audioSource->getInstName()),
                            audioSource->getSampleRate(),
                            audioSource->getChannels(),
                            isFloat ? 32 : audioSource->getNBits(),
                            audioSource->getNBPS(),
                            isFloat,
                        });
                        PendingInput input;
                        input.frameBytes = static_cast<std::size_t>(audioSource->getChannels()) * audioSource->getNBPS();
                        input.maxChunk = MAX_CHUNK_BYTES / input.frameBytes * input.frameBytes;
                        input.chunkLimit = input.maxChunk;
                        _pendingInputs.push_back(std::move(input));
                        _audioSources.push_back(audioSource);
                    } else if (auto externalSource = dynamic_cast<cExternalSource*>(component)) {
                        // Still needs end of input at Finish, otherwise the final tick loop would wait for it forever.
                        _unsupportedSources.push_back(externalSource);
                        AddLog(LOG_WARNING, L"cExternalSource '" + FromAnsi(externalSource->getInstName()) + L"' is not supported and never receives data.");
                    } else if (auto sink = dynamic_cast<cExternalSink*>(component)) {
                        SinkInfo info;
                        info.name = FromAnsi(sink->getInstName());
                        for (long element = 0; element < sink->getNumElements(); element++) {
                            info.elementNames.push_back(FromAnsi(sink->getElementName(element)));
                        }
                        _sinkInfos.push_back(std::move(info));

                        // A sink without a callback still consumes its input, so callbacks are registered before the first tick.
                        std::unique_ptr<SinkContext> context(new SinkContext{ this, static_cast<int>(_sinks.size()) });
                        sink->setDataCallbackEx(&Impl::OnSinkData, context.get());
                        _sinkContexts.push_back(std::move(context));
                        _sinks.push_back(sink);
                    }
                }
            }

            void ReleaseComponents() {
                // The component manager goes first: components keep pointers into configuration memory.
                _componentManager.reset();
                _configManager.reset();
                _commandLine.reset();
            }
#pragma endregion

#pragma region Processing
            void Write(int sourceIndex, const std::uint8_t* data, std::size_t length) {
                if (sourceIndex < 0 || sourceIndex >= static_cast<int>(_audioSources.size())) {
                    throw ExtractorError(ErrorKind::InvalidArgument, L"Source index is out of range.");
                }
                auto& input = _pendingInputs[sourceIndex];
                if (length % input.frameBytes != 0) {
                    // openSMILE would drop the incomplete sample frame and only report it to stderr.
                    throw ExtractorError(ErrorKind::InvalidArgument, L"Data length must be a multiple of the sample frame size (" + std::to_wstring(input.frameBytes) + L" bytes).");
                }
                ThrowIfNotRunning();
                if (_currentState == State::Terminated || length == 0) {
                    return;
                }

                input.Append(data, length);
                if (!DrainPendingInputs()) {
                    // Every source holds data that does not fit, so nothing downstream can make progress.
                    if (std::all_of(_pendingInputs.begin(), _pendingInputs.end(), [](const PendingInput& pending) { return pending.Remaining() > 0; })) {
                        throw ExtractorError(ErrorKind::OpenSmile, L"No external audio source can accept data although all of them have pending input. The data memory levels are too small for the configured processing.");
                    }
                }
            }

            void Finish() {
                ThrowIfNotRunning();
                if (_currentState == State::Terminated) {
                    _currentState = State::Finished;
                    return;
                }

                // A source ends once all of its data is written. Ending one source may unblock another one that waits for it.
                std::vector<bool> ended(_audioSources.size(), false);
                while (true) {
                    auto drained = DrainPendingInputs();
                    if (_currentState == State::Terminated) {
                        _currentState = State::Finished;
                        return;
                    }
                    auto anyEnded = false;
                    for (std::size_t i = 0; i < _audioSources.size(); i++) {
                        if (!ended[i] && _pendingInputs[i].Remaining() == 0) {
                            _audioSources[i]->setExternalEOI();
                            ended[i] = true;
                            anyEnded = true;
                        }
                    }
                    if (drained) {
                        break;
                    }
                    if (!anyEnded) {
                        for (std::size_t i = 0; i < _audioSources.size(); i++) {
                            if (_pendingInputs[i].Remaining() > 0) {
                                AddLog(LOG_WARNING, L"Discarded " + std::to_wstring(_pendingInputs[i].Remaining()) + L" bytes of source '" + _sourceInfos[i].name + L"' that could not be written before the end of input.");
                                _pendingInputs[i].Clear();
                            }
                        }
                        break;
                    }
                    Pump();
                }

                for (std::size_t i = 0; i < _audioSources.size(); i++) {
                    if (!ended[i]) {
                        _audioSources[i]->setExternalEOI();
                    }
                }
                for (auto source : _unsupportedSources) {
                    source->setExternalEOI();
                }

                // With every external source at end of input, nothing reports "waiting for external data" anymore,
                // so the official loop cannot block. It performs the end-of-input passes exactly as SMILExtract does.
                {
                    ThreadPriorityScope priority;
                    _componentManager->runSingleThreaded(-1);
                }
                ThrowIfSinkError();
                _currentState = State::Finished;
            }

            /// <summary>
            /// Writes pending data of all sources, processing after each write. Returns true if everything has been written.
            /// </summary>
            bool DrainPendingInputs() {
                while (true) {
                    auto anyWritten = false;
                    for (std::size_t i = 0; i < _audioSources.size(); i++) {
                        while (_pendingInputs[i].Remaining() > 0) {
                            auto result = TryWriteChunk(i);
                            if (result == WriteResult::Blocked) {
                                break;
                            }
                            if (result == WriteResult::Written) {
                                anyWritten = true;
                                Pump();
                                if (_currentState == State::Terminated) {
                                    for (auto& input : _pendingInputs) {
                                        input.Clear();
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                    if (std::all_of(_pendingInputs.begin(), _pendingInputs.end(), [](const PendingInput& input) { return input.Remaining() == 0; })) {
                        return true;
                    }
                    if (!anyWritten) {
                        return false;
                    }
                    // Processing after a write may have made room for a source that was blocked earlier in this round.
                }
            }

            WriteResult TryWriteChunk(std::size_t sourceIndex) {
                auto& input = _pendingInputs[sourceIndex];
                auto size = (std::min)(input.Remaining(), input.chunkLimit);
                if (_audioSources[sourceIndex]->writeData(input.Current(), static_cast<int>(size))) {
                    input.Consume(size);
                    // The level may have room again after processing, so try larger chunks again.
                    input.chunkLimit = (std::min)(input.chunkLimit * 2, input.maxChunk);
                    return WriteResult::Written;
                }
                // writeData accepts nothing when the chunk exceeds the free space of the level. The real capacity is unknown
                // (dataMemory may enlarge levels beyond the configuration), so the chunk is halved until it fits.
                if (input.chunkLimit > input.frameBytes) {
                    input.chunkLimit = (std::max)(input.frameBytes, input.chunkLimit / 2 / input.frameBytes * input.frameBytes);
                    return WriteResult::Shrunk;
                }
                return WriteResult::Blocked;
            }

            /// <summary>
            /// Ticks until no component performs work. Unlike the official loop, it never enters end of input and never waits for external data.
            /// </summary>
            void Pump() {
                long counts[NUM_TICK_RESULTS];
                long lastRunCount = -1;
                for (long long i = 0; i < MAX_TICKS_PER_PUMP; i++) {
                    _componentManager->tick(-1, _tickNumber++, lastRunCount, counts);
                    if (_componentManager->isAbort()) {
                        _currentState = State::Terminated;
                        return;
                    }
                    lastRunCount = counts[TICK_SUCCESS];
                    if (lastRunCount == 0) {
                        ThrowIfSinkError();
                        return;
                    }
                }
                throw ExtractorError(ErrorKind::OpenSmile, L"openSMILE did not settle after " + std::to_wstring(MAX_TICKS_PER_PUMP) + L" ticks.");
            }

            static bool OnSinkData(const FLOAT_DMEM* data, long frameCount, long elementCount, const sExternalSinkMetaDataEx* metaData, void* parameter) {
                auto context = static_cast<SinkContext*>(parameter);
                auto owner = context->owner;
                // The metadata describes the first frame of the block only. Later frames can be derived from a fixed period only.
                if (frameCount > 1 && metaData->period <= 0) {
                    if (owner->_sinkError.empty()) {
                        owner->_sinkError = L"Sink '" + owner->_sinkInfos[context->sinkIndex].name + L"' received a block of " + std::to_wstring(frameCount) + L" frames from a level without a fixed period, so the frame times are unknown. Set blocksize = 1 for this sink.";
                    }
                    return true;
                }
                for (long t = 0; t < frameCount; t++) {
                    auto values = data + t * elementCount;
                    owner->_frames.push_back(Frame{
                        context->sinkIndex,
                        static_cast<long long>(metaData->vIdx) + t,
                        metaData->time + t * metaData->period,
                        metaData->period,
                        metaData->lengthSec,
                        std::vector<float>(values, values + elementCount),
                    });
                }
                return true;
            }
#pragma endregion

#pragma region Helpers
            void ThrowIfNotRunning() const {
                if (_currentState == State::Finished) {
                    throw ExtractorError(ErrorKind::InvalidOperation, L"The extractor has already finished.");
                }
                if (_currentState == State::Faulted) {
                    throw ExtractorError(ErrorKind::InvalidOperation, L"The extractor is faulted by an earlier error.");
                }
            }

            void ThrowIfSinkError() {
                if (!_sinkError.empty()) {
                    throw ExtractorError(ErrorKind::OpenSmile, _sinkError);
                }
            }

            void AddLog(int type, const std::wstring& text) {
                _logs->push_back(LogEntry{ type, 0, L"OpenSmileInterop", text });
            }

            /// <summary>
            /// Runs an operation and converts its exceptions to ExtractorError. Errors from openSMILE leave the instance in an unknown state, so it becomes faulted.
            /// </summary>
            template <typename TAction>
            void Execute(TAction action) {
                auto firstLog = _logs->size();
                try {
                    action();
                } catch (const ExtractorError& ex) {
                    if (ex.kind == ErrorKind::OpenSmile) {
                        _currentState = State::Faulted;
                    }
                    throw;
                } catch (const cSMILException& ex) {
                    // Caught by reference: copies of openSMILE exceptions share and double-free their text.
                    _currentState = State::Faulted;
                    throw ExtractorError(ErrorKind::OpenSmile, ComposeErrorMessage(firstLog, ex.getType()));
                } catch (const std::exception& ex) {
                    _currentState = State::Faulted;
                    throw ExtractorError(ErrorKind::OpenSmile, FromAnsi(ex.what()));
                }
            }

            std::wstring ComposeErrorMessage(std::size_t firstLog, int exceptionType) const {
                // The useful text is often an earlier error, e.g. "unknown component type" comes before "error during addComponent".
                std::wstring message;
                for (auto i = firstLog; i < _logs->size(); i++) {
                    const auto& entry = (*_logs)[i];
                    if (entry.type != LOG_ERROR) {
                        continue;
                    }
                    if (!message.empty()) {
                        message += L"\n";
                    }
                    if (!entry.module.empty()) {
                        message += entry.module + L": ";
                    }
                    message += entry.text;
                }
                if (message.empty()) {
                    message = L"openSMILE raised an exception of type " + std::to_wstring(exceptionType) + L" without logging an error.";
                }
                return message;
            }
#pragma endregion
        };

        NativeExtractor::NativeExtractor(
            const std::wstring& configFilename,
            const std::vector<std::pair<std::wstring, std::wstring>>& options,
            int logLevel,
            std::vector<LogEntry>* logs
        )
            : _impl(new Impl(configFilename, options, logLevel, logs)) {
        }

        NativeExtractor::~NativeExtractor() = default;

        const std::vector<SourceInfo>& NativeExtractor::GetSources() const {
            return _impl->_sourceInfos;
        }

        const std::vector<SinkInfo>& NativeExtractor::GetSinks() const {
            return _impl->_sinkInfos;
        }

        bool NativeExtractor::IsTerminated() const {
            return _impl->_currentState == State::Terminated;
        }

        void NativeExtractor::Write(int sourceIndex, const std::uint8_t* data, std::size_t length) {
            ThreadStateScope scope(_impl->_logger.get());
            _impl->Execute([&]() {
                _impl->Write(sourceIndex, data, length);
            });
        }

        void NativeExtractor::Finish() {
            ThreadStateScope scope(_impl->_logger.get());
            _impl->Execute([&]() {
                _impl->Finish();
            });
        }

        void NativeExtractor::TakeFrames(std::vector<Frame>& output) {
            output.clear();
            output.swap(_impl->_frames);
        }
    }
}
