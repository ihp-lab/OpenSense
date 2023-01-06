using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Biopac {
    [Export(typeof(IComponentMetadata))]
    public class BiopacMetadata : ConventionalComponentMetadata {

        public override string Description => "Use with Biopac to measure heart beats. Need refactor.";

        protected override Type ComponentType => typeof(Biopac);

        public override string Name => "Biopac Sensor";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(Biopac.Out):
                    return "Output value of Biopac. The output type is string (why?).";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BiopacConfiguration();
    }
}
