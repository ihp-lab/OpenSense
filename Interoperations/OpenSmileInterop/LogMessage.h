#pragma once

#include "Enums.h"

using namespace System;

namespace OpenSmileInterop {
    /// <summary>
    /// A log message emitted by openSMILE
    /// </summary>
    public ref class LogMessage sealed {
    private:
        initonly LogType _type;
        initonly int _level;
        initonly String^ _module;
        initonly String^ _text;

    internal:
        LogMessage(LogType type, int level, String^ module, String^ text)
            : _type(type), _level(level), _module(module), _text(text) {
        }

    public:
        property LogType Type {
            LogType get() { return _type; }
        }

        /// <summary>
        /// Verbosity level of the message. Higher values are more detailed.
        /// </summary>
        property int Level {
            int get() { return _level; }
        }

        /// <summary>
        /// The openSMILE module or component instance that emitted the message. May be empty.
        /// </summary>
        property String^ Module {
            String^ get() { return _module; }
        }

        property String^ Text {
            String^ get() { return _text; }
        }
    };
}
