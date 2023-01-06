using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ColorVideoVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes color images.";

        protected override Type ComponentType => typeof(ColorVideoVisualizer);

        public override string Name => "Color Image Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ColorVideoVisualizer.In):
                    return "[Required] Color images to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ColorVideoVisualizerConfiguration();
    }
}
