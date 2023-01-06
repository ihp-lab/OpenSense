using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.AzureKinect.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectBodyTrackerVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes Azure Kinect body tracker's outputs.";

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
