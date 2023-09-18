using System;
using System.Composition;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Components.Psi.AzureKinect {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description => "Azure Kinect sensor.";

        protected override Type ComponentType => typeof(AzureKinectSensor);

        public override string Name => "Azure Kinect Sensor";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectSensor.ColorImage):
                    return "Color images. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.InfraredImage):
                    return "Infrared images. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.DepthImage):
                    return "Depth images. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.AzureKinectSensorCalibration):
                    return "Sensor calibration information. Send only once. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.DepthDeviceCalibrationInfo):
                    return "Depth calibration information. Send only once. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.Temperature):
                    return "Temperatures.";
                case nameof(AzureKinectSensor.Imu):
                    return "IMU samples. Only available when enabled in the sensor configuration.";
                case nameof(AzureKinectSensor.FrameRate):
                    return "Current framerates.";
                case nameof(AzureKinectSensor.Bodies):
                    return "Tracked bodies. Only available when enabled in the sensor configuration.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectSensorConfiguration();
    }
}
