#pragma once

using namespace System;

namespace OpenSmileInterop {
    /// <summary>
    /// Type of an openSMILE log message (maps to LOG_* in smileLogger.hpp)
    /// </summary>
    public enum class LogType : int {
        Message = 1,  // LOG_MESSAGE
        Warning = 2,  // LOG_WARNING
        Error = 3,    // LOG_ERROR
        Debug = 4,    // LOG_DEBUG, disabled because openSMILE's debug output is unreliable (see NativeExtractor.cpp)
        Print = 5,    // LOG_PRINT, unformatted output such as level statistics
    };
}
