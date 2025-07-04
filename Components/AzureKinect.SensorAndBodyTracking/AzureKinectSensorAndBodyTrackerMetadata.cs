using System;
using System.Composition;
using OpenSense.Components.AzureKinect.BodyTracking;

namespace OpenSense.Components.AzureKinect.SensorAndBodyTracking {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureKinectSensorAndBodyTrackerMetadata : ConventionalComponentMetadata {

        public override string Description =>
            "This component combines own Azure Kinect Sensor and Body Tracker into a single unit, avoiding exposure of native types to potentially reduce tracker instability. Limitations of individual components apply."
            ;

        protected override Type ComponentType => typeof(AzureKinectSensorAndBodyTracker);

        public override string Name => "Azure Kinect Sensor & Body Tracker";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureKinectSensorAndBodyTracker.CalibrationOut):
                    return "The calibration data returned by the SDK. Only one instance will be outputted, and its timestamp will be the same as the pipeline starting time.";
                case nameof(AzureKinectSensorAndBodyTracker.PsiCalibrationOut):
                    return "The calibration data in \\psi's data type. Only one instance will be outputted, and its timestamp will be the same as the pipeline starting time.";
                case nameof(AzureKinectSensorAndBodyTracker.PsiColorOut):
                    return "The color image in \\psi's data type. Only when the Color Format is set to BGRA32. The pixel format is 8-bit BGRA.";
                case nameof(AzureKinectSensorAndBodyTracker.PsiDepthOut):
                    return "The depth image in \\psi's data type. The size is dependent on the Depth Mode.";
                case nameof(AzureKinectSensorAndBodyTracker.PsiInfraredOut):
                    return "The infrared image in \\psi's data type. The pixel format is 16-bit grayscale. The size is dependent on the Depth Mode.";
                case nameof(AzureKinectSensorAndBodyTracker.CaptureTemperatureOut):
                    return "The temperature from the capture device. The unit is Celsius.";
                case nameof(AzureKinectSensorAndBodyTracker.ImuOut):
                    return "The IMU data object returned by the SDK. It contains temperature, acceleration, gyroscope data and their timestamps.";
                case nameof(AzureKinectSensorAndBodyTracker.ImuTemperatureOut):
                    return "The temperature from the IMU. The unit is Celsius.";
                case nameof(AzureKinectBodyTracker.BodiesOut):
                    return "A array of detected bodies in \\psi's coordinate convention. The value can be null when timeout.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectSensorAndBodyTrackerConfiguration();
    }
}
