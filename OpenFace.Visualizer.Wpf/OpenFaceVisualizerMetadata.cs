using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenFace.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenFaceVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize OpenFace results.";

        protected override Type ComponentType => typeof(OpenFaceVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new OpenFaceVisualizerConfiguration();
    }
}
