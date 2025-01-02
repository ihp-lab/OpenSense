using System;
using System.Composition;

namespace OpenSense.Components.SerialPorts {
    [Export(typeof(IComponentMetadata))]
    public class SerialPortPinPullerMetadata : ConventionalComponentMetadata {

        public override string Name => "Serial Port DTR/RTS Pin Puller";

        public override string Description => "Open a Serial Port and drive its DTR/RTS pin.";

        protected override Type ComponentType => typeof(SerialPortPinPuller);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(SerialPortPinPuller.DtrIn):
                    return "[Optional] DTR pin driving input signal.";
                case nameof(SerialPortPinPuller.DtrOut):
                    return "DTR pin’s state whenever a state transition occurs.";
                case nameof(SerialPortPinPuller.RtsIn):
                    return "[Optional] RTS pin driving input signal.";
                case nameof(SerialPortPinPuller.RtsOut):
                    return "RTS pin’s state whenever a state transition occurs.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new SerialPortPinPullerConfiguration();
    }
}
