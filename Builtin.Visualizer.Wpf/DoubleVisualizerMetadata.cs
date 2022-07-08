using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DoubleVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize double floating point values.";

        protected override Type ComponentType => typeof(DoubleVisualizer);

        public override string Name => "Double Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DoubleVisualizer.In):
                    return "[Required] The double floating point values to be visualized.";
                case nameof(DoubleVisualizer.Out):
                    return "Double floating point mapped texts.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DoubleVisualizerConfiguration();
    }
}
