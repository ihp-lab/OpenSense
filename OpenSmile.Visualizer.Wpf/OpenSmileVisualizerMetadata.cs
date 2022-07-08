using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenSmile.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenSmileVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes openSMILE outputs. Requires openSMILE outputs.";

        protected override Type ComponentType => typeof(OpenSmileVisualizer);

        public override string Name => "openSMILE Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenSmileVisualizer.In):
                    return "[Required] openSMILE outputs.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenSmileVisualizerConfiguration();
    }
}
