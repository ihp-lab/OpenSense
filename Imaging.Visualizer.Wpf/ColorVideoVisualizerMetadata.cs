using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ColorVideoVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize color video.";

        protected override Type ComponentType => typeof(ColorVideoVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new ColorVideoVisualizerConfiguration();
    }
}
