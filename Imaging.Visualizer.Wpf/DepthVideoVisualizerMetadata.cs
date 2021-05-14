using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DepthVideoVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize depth video.";

        protected override Type ComponentType => typeof(DepthVideoVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new DepthVideoVisualizerConfiguration();
    }
}
