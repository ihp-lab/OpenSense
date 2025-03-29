using System;
using System.Composition;

namespace OpenSense.Components.AzureKinect.Sensor {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureKinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description =>
            "This is our own Azure Kinect Sensor component implementation, intended to address the performance, compatibility and feature richness issues of \\psi's official implementation. Body tracking is not included. Detailed device specifications can be found at: https://learn.microsoft.com/en-us/previous-versions/azure/kinect-dk."
            ;

        protected override Type ComponentType => typeof(AzureKinectSensor);

        public override string Name => "Azure Kinect Sensor";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectSensor.CalibrationOut):
                    return "The calibration data returned by the SDK. Only one instance will be outputted, and its timestamp will be the same as the pipeline starting time.";
                case nameof(AzureKinectSensor.PsiCalibrationOut):
                    return "The calibration data in \\psi's data type. Only one instance will be outputted, and its timestamp will be the same as the pipeline starting time.";
                case nameof(AzureKinectSensor.CaptureOut):
                    return "The capture data object returned by the SDK. It contains temperature, detailed color image, depth image and infrared image. IMU data is not included.";
                case nameof(AzureKinectSensor.ColorOut):
                    return "The color image returned by the SDK";
                case nameof(AzureKinectSensor.DepthOut):
                    return "The depth image returned by the SDK";
                case nameof(AzureKinectSensor.InfraredOut):
                    return "The infrared image returned by the SDK";
                case nameof(AzureKinectSensor.PsiColorOut):
                    return "The color image in \\psi's data type. Only when the Color Format is set to BGRA32. The pixel format is 8-bit BGRA.";
                case nameof(AzureKinectSensor.PsiDepthOut):
                    return "The depth image in \\psi's data type. The size is dependent on the Depth Mode.";
                case nameof(AzureKinectSensor.PsiInfraredOut):
                    return "The infrared image in \\psi's data type. The pixel format is 16-bit grayscale. The size is dependent on the Depth Mode.";
                case nameof(AzureKinectSensor.CaptureTemperatureOut):
                    return "The temperature from the capture device. The unit is Celsius.";
                case nameof(AzureKinectSensor.ImuOut):
                    return "The IMU data object returned by the SDK. It contains temperature, acceleration, gyroscope data and their timestamps.";
                case nameof(AzureKinectSensor.ImuTemperatureOut):
                    return "The temperature from the IMU. The unit is Celsius.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectSensorConfiguration();
    }
}
