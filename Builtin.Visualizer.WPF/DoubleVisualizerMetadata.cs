using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DoubleVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes floating point values. Accepts single or double precision.";

        protected override Type ComponentType => typeof(DoubleVisualizer);

        public override string Name => "Floating Point Value Visualizer";

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
