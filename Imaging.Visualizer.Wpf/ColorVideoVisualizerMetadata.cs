using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ColorVideoVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize color images.";

        protected override Type ComponentType => typeof(ColorVideoVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new ColorVideoVisualizerConfiguration();
    }
}
