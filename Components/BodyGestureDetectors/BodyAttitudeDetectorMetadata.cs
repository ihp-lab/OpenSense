﻿using System;
using System.Composition;

namespace OpenSense.Components.BodyGestureDetectors {
    [Export(typeof(IComponentMetadata))]
    public class BodyAttitudeDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "[Experimental] Detects body attitude (yaw, pitch, roll). Requires Azure Kinect outputs.";

        protected override Type ComponentType => typeof(BodyAttitudeDetector);

        public override string Name => "Body Attitude Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(BodyAttitudeDetector.ImuIn):
                    return "[Required] IMU samples of Azure Kinect. Make sure Azure Kinect's IMU output is enabled.";
                case nameof(BodyAttitudeDetector.BodiesIn):
                    return "[Required] Tracked bodies of Azure Kinect.";

                case nameof(BodyAttitudeDetector.PitchDegreeOut):
                    return "Floating point pitch angle in degrees (forward negative, backward positive).";
                case nameof(BodyAttitudeDetector.PitchRadianOut):
                    return "Floating point pitch angle in radians (forward negative, backward positive).";
                case nameof(BodyAttitudeDetector.YawDegreeOut):
                    return "Floating point yaw angle in degrees (left negative, right positive).";
                case nameof(BodyAttitudeDetector.YawRadianOut):
                    return "Floating point yaw angle in radians (left negative, right positive).";
                case nameof(BodyAttitudeDetector.RollDegreeOut):
                    return "Floating point roll angle in degrees (left negative, right positive).";
                case nameof(BodyAttitudeDetector.RollRadianOut):
                    return "Floating point roll angle in radians (left negative, right positive).";

                case nameof(BodyAttitudeDetector.HeadPitchDegreeOut):
                    return "Floating point pitch angle in degrees (forward negative, backward positive).";
                case nameof(BodyAttitudeDetector.HeadPitchRadianOut):
                    return "Floating point pitch angle in radians (forward negative, backward positive).";
                case nameof(BodyAttitudeDetector.HeadYawDegreeOut):
                    return "Floating point yaw angle in degrees (left negative, right positive).";
                case nameof(BodyAttitudeDetector.HeadYawRadianOut):
                    return "Floating point yaw angle in radians (left negative, right positive).";
                case nameof(BodyAttitudeDetector.HeadRollDegreeOut):
                    return "Floating point roll angle in degrees (left negative, right positive).";
                case nameof(BodyAttitudeDetector.HeadRollRadianOut):
                    return "Floating point roll angle in radians (left negative, right positive).";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BodyAttitudeDetectorConfiguration();
    }
}
