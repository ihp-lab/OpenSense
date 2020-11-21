using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Builtin.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class BooleanVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize boolean values.";

        protected override Type ComponentType => typeof(BooleanVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new BooleanVisualizerConfiguration();
    }
}
