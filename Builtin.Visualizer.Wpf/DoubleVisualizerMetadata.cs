using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DoubleVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize boolean values.";

        protected override Type ComponentType => typeof(DoubleVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new DoubleVisualizerConfiguration();
    }
}
