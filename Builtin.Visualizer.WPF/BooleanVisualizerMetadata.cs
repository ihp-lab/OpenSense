using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class BooleanVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize boolean values.";

        protected override Type ComponentType => typeof(BooleanVisualizer);

        public override string Name => "Boolean Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(BooleanVisualizer.In):
                    return "[Required] Boolean values to be visualized.";
                case nameof(BooleanVisualizer.Out):
                    return "Boolean mapped texts.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BooleanVisualizerConfiguration();
    }
}
