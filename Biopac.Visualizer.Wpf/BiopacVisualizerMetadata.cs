using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Biopac.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class BiopacVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Biopac results.";

        protected override Type ComponentType => typeof(BiopacVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new BiopacVisualizerConfiguration();
    }
}
