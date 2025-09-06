#include "pch.h"

#include <msclr\marshal_cppstd.h>

#include "Config.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace KvazaarInterop {
    Config::Config()
        : _config(nullptr)
        , _disposed(false) {

        auto api = Api::GetApi();
        _config = api->config_alloc();

        if (!_config) {
            throw gcnew OutOfMemoryException("Failed to allocate config");
        }

        auto result = api->config_init(_config);
        if (!result) {
            api->config_destroy(_config);
            _config = nullptr;
            throw gcnew InvalidOperationException("Failed to initialize config");
        }
    }

    bool Config::Parse([NotNull] String^ name, [NotNull] String^ value) {
        ThrowIfDisposed();

        ArgumentNullException::ThrowIfNull(name, "name");
        ArgumentNullException::ThrowIfNull(value, "value");

        auto api = Api::GetApi();
        auto nativeName = marshal_as<std::string>(name);
        auto nativeValue = marshal_as<std::string>(value);

        auto result = api->config_parse(_config, nativeName.c_str(), nativeValue.c_str());
        return result != 0;
    }

    void Config::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Config::~Config() {
        if (_disposed) {
            return;
        }

        this->!Config();
        _disposed = true;
    }

    Config::!Config() {
        if (_config) {
            auto api = Api::GetApi();
            api->config_destroy(_config);
            _config = nullptr;
        }
    }
}