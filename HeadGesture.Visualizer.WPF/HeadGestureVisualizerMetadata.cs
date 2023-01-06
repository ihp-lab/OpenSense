using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.HeadGesture.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class HeadGestureVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes head gesture outputs. Shows the name of head gesture values. Requries head gesture detector outputs.";

        protected override Type ComponentType => typeof(HeadGestureVisualizer);

        public override string Name => "Head Gesture Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(HeadGestureVisualizer.In):
                    return "[Required] Head gesture outputs to be visualized.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new HeadGestureVisualizerConfiguration();
    }
}
