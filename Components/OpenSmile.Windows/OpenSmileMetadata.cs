using System;
using System.Composition;

namespace OpenSense.Components.OpenSmile {
    [Export(typeof(IComponentMetadata))]
    public class OpenSmileMetadata : ConventionalComponentMetadata {

        public override string Description => "openSMILE by audEERING GmbH for signal processing. Requires openSMILE pipeline configuration file.";

        protected override Type ComponentType => typeof(OpenSmile);

        public override string Name => "openSMILE";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenSmile.In):
                    return "[Required] Named audio signals. Name should match the source component name in the openSMILE configuration file.";
                case nameof(OpenSmile.Out):
                    return "[Composite] Named data sinks. Name should match the sink component name in the openSMILE configuration file.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenSmileConfiguration();
    }
}
