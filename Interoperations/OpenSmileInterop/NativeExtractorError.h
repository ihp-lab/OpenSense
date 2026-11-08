#pragma once

// Shared by managed and native translation units, so it must not include openSMILE headers.
// See NativeExtractor.cpp in OpenSmileInterop.vcxproj for the reason.

#include <string>
#include <utility>

namespace OpenSmileInterop {
    namespace Native {

        enum class ErrorKind {
            OpenSmile,
            InvalidArgument,
            InvalidOperation,
        };

        /// <summary>
        /// Thrown by NativeExtractor. Carries a wide message because openSMILE error texts are converted from the ANSI code page.
        /// </summary>
        class ExtractorError {
        public:
            ExtractorError(ErrorKind kind, std::wstring message) : kind(kind), message(std::move(message)) {}

            ErrorKind kind;
            std::wstring message;
        };
    }
}
