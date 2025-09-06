#include "pch.h"
#include "Api.h"

using namespace System::Threading;

namespace KvazaarInterop {
    int Api::BitDepth::get() {
        return _bitDepth;
    }

    void Api::BitDepth::set(int value) {
        Monitor::Enter(_lock);
        try {
            if (_initialized) {
                throw gcnew InvalidOperationException(
                    "Cannot change BitDepth after API has been initialized. "
                    "Set BitDepth before creating any Encoder, Config, or Picture instances.");
            }
            
            // Validate bit depth value
            if (value != 8 && value != 10 && value != 12 && value != 16) {
                throw gcnew ArgumentException(
                    "Invalid bit depth. Supported values are 8, 10, 12, or 16.");
            }
            
            _bitDepth = value;
        }
        finally {
            Monitor::Exit(_lock);
        }
    }

    const kvz_api* Api::GetApi() {
        if (!_initialized) {
            Monitor::Enter(_lock);
            try {
                if (!_initialized) {
                    Initialize();
                }
            }
            finally {
                Monitor::Exit(_lock);
            }
        }
        return _api;
    }

    void Api::Initialize() {
        _api = kvz_api_get(_bitDepth);
        if (!_api) {
            throw gcnew InvalidOperationException(
                String::Format("Failed to get Kvazaar API for bit depth {0}", _bitDepth));
        }
        _initialized = true;
    }
}