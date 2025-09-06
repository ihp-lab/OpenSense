#include "pch.h"
#include "Api.h"

namespace KvazaarInterop {
    static Api::Api() {
        _api = kvz_api_get(KVZ_BIT_DEPTH);//The bit depth parameter is ignored in the implementation, it always returns the same instance called "kvz_8bit_api", though internally the bit depth is determined by the KVZ_BIT_DEPTH macro.
        if (!_api) {
            throw gcnew InvalidOperationException("Failed to get Kvazaar API");
        }
    }

    const kvz_api* Api::GetApi() {
        return _api;
    }
}