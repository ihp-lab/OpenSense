using System;
using System.Composition;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Components.Psi.AzureKinect {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectBodyTrackerMetadata : ConventionalComponentMetadata {

        public override string Description => "[Deprecated] Azure Kinect body tracker. Requires outputs from Azure Kinect. The Azure Kinect Sensor component already has this tracker built-in. Microsoft has dropped support for this device, and its SDK has compatibility issues with other components.";

        protected override Type ComponentType => typeof(AzureKinectBodyTracker);

        public override string Name => "Azure Kinect Body Tracker";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectBodyTracker.In):
                    return "[Required] Paird depth image and IR image.";
                case nameof(AzureKinectBodyTracker.AzureKinectSensorCalibration):
                    return "[Required] Sensor calibration information. Required only once for initialization.";
                case nameof(AzureKinectBodyTracker.Out):
                    return "Tracked bodies.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerConfiguration();
    }
}
