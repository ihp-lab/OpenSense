using System;
using System.Composition;

namespace OpenSense.Components.OpenPose.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenPoseVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes OpenPose outputs. Requires OpenPose outputs.";

        protected override Type ComponentType => typeof(OpenPoseVisualizer);

        public override string Name => "OpenPose Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenPoseVisualizer.DataIn):
                    return "[Required] OpenPose outputs.";
                case nameof(OpenPoseVisualizer.ImageIn):
                    return "[Required] Images that were send to OpenFace.";
                case nameof(OpenPoseVisualizer.Out):
                    return "Rendered images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenPoseVisualizerConfiguration();
    }
}
