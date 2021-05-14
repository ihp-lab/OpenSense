using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.HeadGesture.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class HeadGestureVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize head gesture data.";

        protected override Type ComponentType => typeof(HeadGestureVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new HeadGestureVisualizerConfiguration();
    }
}
