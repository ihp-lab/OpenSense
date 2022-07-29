using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.BodyGestureDetectors {
    [Export(typeof(IComponentMetadata))]
    public class BodyLeaningDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detects body leaning angle. Requires Azure Kinect outputs.";

        protected override Type ComponentType => typeof(BodyLeaningDetector);

        public override string Name => "Body Leaning Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(BodyLeaningDetector.ImuIn):
                    return "[Required] IMU samples of Azure Kinect.";
                case nameof(BodyLeaningDetector.BodiesIn):
                    return "[Required] Tracked bodies of Azure Kinect.";
                case nameof(BodyLeaningDetector.DegreeOut):
                    return "Floating point angle in degrees (forward negative, backward positive).";
                case nameof(BodyLeaningDetector.Out):
                    return "Floating point angle in radians (forward negative, backward positive).";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BodyLeaningDetectorConfiguration();
    }
}
