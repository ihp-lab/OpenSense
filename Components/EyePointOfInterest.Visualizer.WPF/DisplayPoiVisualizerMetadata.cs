using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.EyePointOfInterest.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DisplayPoiVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes point of interest outputs. Shows location in a rectangle. Requires display POI estimator outputs.";

        protected override Type ComponentType => typeof(DisplayPoiVisualizer);

        public override string Name => "Display POI Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DisplayPoiVisualizer.In):
                    return "[Required] Normalized POI locations.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DisplayPoiVisualizerConfiguration();
    }
}
