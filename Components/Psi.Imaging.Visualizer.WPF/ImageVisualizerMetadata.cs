using System;
using System.Composition;

namespace OpenSense.Components.Psi.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ImageVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes images.";

        protected override Type ComponentType => typeof(ImageVisualizer);

        public override string Name => "Image Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ImageVisualizer.In):
                    return "[Required] Images to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ImageVisualizerConfiguration();
    }
}
