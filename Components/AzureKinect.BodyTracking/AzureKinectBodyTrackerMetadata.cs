using System;
using System.Composition;

namespace OpenSense.Components.AzureKinect.BodyTracking {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureKinectBodyTrackerMetadata : ConventionalComponentMetadata {

        public override string Description =>
            "This is our own Azure Kinect Body Tracker component implementation, created to improve performance and fix compatibility issues with \\psi's official version. Only one tracker can run per process. It relies on our Azure Kinect Sensor component. This component may not work out-of-box."
            ;

        protected override Type ComponentType => typeof(AzureKinectBodyTracker);

        public override string Name => "Azure Kinect Body Tracker";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectBodyTracker.CalibrationIn):
                    return "The calibration data.";
                case nameof(AzureKinectBodyTracker.CaptureIn):
                    return "The capture data.";
                case nameof(AzureKinectBodyTracker.FrameOut):
                    return "The output of the tracker. The content of Shared<> can be null when timeout.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerConfiguration();
    }
}
