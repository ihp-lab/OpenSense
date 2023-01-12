using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DepthImageVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes depth images.";

        protected override Type ComponentType => typeof(DepthImageVisualizer);

        public override string Name => "Depth Image Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DepthImageVisualizer.In):
                    return "[Required] Depth images to be visualized.";
                case nameof(DepthImageVisualizer.Out):
                    return "Rendered color images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DepthImageVisualizerConfiguration();
    }
}
