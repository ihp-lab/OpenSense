using System;
using System.Composition;

namespace OpenSense.Components.Psi.AzureKinect.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectBodyTrackerVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "[Deprecated] Visualizes Azure Kinect body tracker's outputs. This is for \\psi's Azure Kinect implementation.";

        protected override Type ComponentType => typeof(AzureKinectBodyTrackerVisualizer);

        public override string Name => "Azure Kinect Body Tracker Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectBodyTrackerVisualizer.CalibrationIn):
                    return "[Required] Azure Kinect calibration information.";
                case nameof(AzureKinectBodyTrackerVisualizer.ColorImageIn):
                    return "[Required] Color images.";
                case nameof(AzureKinectBodyTrackerVisualizer.BodiesIn):
                    return "[Required] Tracked bodies.";
                case nameof(AzureKinectBodyTrackerVisualizer.Out):
                    return "Rendered images.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerVisualizerConfiguration();
    }
}
