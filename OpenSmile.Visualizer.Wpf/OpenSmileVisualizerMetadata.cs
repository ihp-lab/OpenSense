using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenSmile.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenSmileVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize OpenSmile results.";

        protected override Type ComponentType => typeof(OpenSmileVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new OpenSmileVisualizerConfiguration();
    }
}
