using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DepthVideoVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes depth images.";

        protected override Type ComponentType => typeof(DepthVideoVisualizer);

        public override string Name => "Depth Image Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DepthVideoVisualizer.In):
                    return "[Required] Depth images to be visualized.";
                case nameof(DepthVideoVisualizer.Out):
                    return "Rendered color images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DepthVideoVisualizerConfiguration();
    }
}
