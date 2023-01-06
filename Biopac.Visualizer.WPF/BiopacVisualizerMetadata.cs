using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Biopac.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class BiopacVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Biopac outputs.";

        protected override Type ComponentType => typeof(BiopacVisualizer);

        public override string Name => "Biopac Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(BiopacVisualizer.In):
                    return "[Required] Biopac outputs.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BiopacVisualizerConfiguration();
    }
}
