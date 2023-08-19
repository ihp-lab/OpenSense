using System;

namespace OpenSense.Components.Contract {
    public sealed class MissingRequiredInputConnectionException : Exception {

        public string ComponentName { get; }

        public string InputPortName { get; }

        public override string Message => $"Missing a required connection for \"{InputPortName}\" of \"{ComponentName}\".";

        public MissingRequiredInputConnectionException(string componentName, string inputPortName) {
            ComponentName = componentName;
            InputPortName = inputPortName;
        }
    }
}
