using System;
using System.Composition;
using Microsoft.Psi.Kinect;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Kinect {
    [Export(typeof(IComponentMetadata))]
    public class KinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description => "Kinect (v2) sensor. Requires a SDK to be installed.";

        protected override Type ComponentType => typeof(KinectSensor);

        public override string Name => "Kinect (v2) Sensor";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(KinectSensor.ColorImage):
                    return "Color images.";
                case nameof(KinectSensor.RGBDImage):
                    return "Color & depth images.";
                case nameof(KinectSensor.DepthImage):
                    return "Depth images.";
                case nameof(KinectSensor.InfraredImage):
                    return "Infrared images.";
                case nameof(KinectSensor.LongExposureInfraredImage):
                    return "Long exposure infrared images.";
                case nameof(KinectSensor.Audio):
                    return "Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new KinectSensorConfiguration();
    }
}
