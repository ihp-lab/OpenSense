using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.EyePointOfInterest.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class DisplayPoiVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizer Display POI results.";

        protected override Type ComponentType => typeof(DisplayPoiVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new DisplayPoiVisualizerConfiguration();
    }
}
