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
    public:
        /// <summary>
        /// Gets or sets the bit depth for encoding
        /// Can only be set before any Encoder, Config, or Picture instances are created.
        /// Default is 8-bit.
        /// The official implementaion actually ignores this value and asumes 8-bit.
        /// </summary>
        static property int BitDepth {
            int get();
            void set(int value);
        }

    internal:
        /// <summary>
        /// Get the kvz_api instance (lazy initialization)
        /// </summary>
        static const kvz_api* GetApi();

    private:
        static const kvz_api* _api = nullptr;
        static int _bitDepth = 8;
        static bool _initialized = false;
        static Object^ _lock = gcnew Object();
        
        static void Initialize();
    };
}