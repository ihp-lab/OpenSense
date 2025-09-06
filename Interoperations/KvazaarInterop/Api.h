#pragma once

extern "C" {
#include <kvazaar.h>
}

using namespace System;

namespace KvazaarInterop {
    /// <summary>
    /// Internal class for managing kvz_api instance
    /// </summary>
    ref class Api abstract sealed {
    internal:
        /// <summary>
        /// Get the kvz_api instance
        /// </summary>
        static const kvz_api* GetApi();

    private:
        static const kvz_api* _api = nullptr;
        static Api();
    };
}