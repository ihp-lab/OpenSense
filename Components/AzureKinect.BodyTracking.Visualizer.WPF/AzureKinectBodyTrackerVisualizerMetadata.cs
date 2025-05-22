using System.Composition;

namespace OpenSense.Components.AzureKinect.BodyTracking.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureKinectBodyTrackerVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes Azure Kinect body tracker's outputs.";

        protected override Type ComponentType => typeof(AzureKinectBodyTrackerVisualizer);

        public override string Name => "Azure Kinect Body Tracker Visualizer";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectBodyTrackerVisualizer.CalibrationIn):
                    return "[Required] Azure Kinect calibration information.";
                case nameof(AzureKinectBodyTrackerVisualizer.In):
                    return "[Required] Paired \\psi image and tracker's bodies output.";
                case nameof(AzureKinectBodyTrackerVisualizer.Out):
                    return "Rendered images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerVisualizerConfiguration();
    }
}
