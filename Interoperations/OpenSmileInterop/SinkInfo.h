#pragma once

using namespace System;
using namespace System::Collections::Generic;

namespace OpenSmileInterop {
    /// <summary>
    /// A cExternalSink instance and the features it outputs
    /// </summary>
    public ref class SinkInfo sealed {
    private:
        initonly String^ _name;
        initonly IReadOnlyList<String^>^ _elementNames;

    internal:
        SinkInfo(String^ name, IReadOnlyList<String^>^ elementNames)
            : _name(name), _elementNames(elementNames) {
        }

    public:
        /// <summary>
        /// Instance name in the configuration file
        /// </summary>
        property String^ Name {
            String^ get() { return _name; }
        }

        /// <summary>
        /// Feature names, one per value of a frame
        /// </summary>
        property IReadOnlyList<String^>^ ElementNames {
            IReadOnlyList<String^>^ get() { return _elementNames; }
        }
    };
}
