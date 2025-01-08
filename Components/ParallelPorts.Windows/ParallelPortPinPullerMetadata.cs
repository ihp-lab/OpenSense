using System;
using System.Composition;

namespace OpenSense.Components.ParallelPorts {
    [Export(typeof(IComponentMetadata))]
    public class ParallelPortPinPullerMetadata : ConventionalComponentMetadata {

        public override string Name => "Parallel Port Pin Puller";

        public override string Description => "Drive a Parallel Port's data pins. Inpoutx64 is used behind, install its driver first.";

        protected override Type ComponentType => typeof(ParallelPortPinPuller);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ParallelPortPinPuller.In):
                    return "[Optional] Data pins driving input signal.";
                case nameof(ParallelPortPinPuller.Out):
                    return "Data pins’ state whenever a state transition occurs.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ParallelPortPinPullerConfiguration();
    }
}
