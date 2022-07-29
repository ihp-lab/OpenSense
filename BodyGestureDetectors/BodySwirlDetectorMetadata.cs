using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.BodyGestureDetectors {
    [Export(typeof(IComponentMetadata))]
    public class BodySwirlDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detects body swirl angle. Requires Azure Kinect outputs.";

        protected override Type ComponentType => typeof(BodySwirlDetector);

        public override string Name => "Body Swril Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(BodySwirlDetector.ImuIn):
                    return "[Required] IMU samples of Azure Kinect.";
                case nameof(BodySwirlDetector.BodiesIn):
                    return "[Required] Tracked bodies of Azure Kinect.";
                case nameof(BodySwirlDetector.DegreeOut):
                    return "Floating point angle in degrees (left negative, right positive).";
                case nameof(BodySwirlDetector.Out):
                    return "Floating point angle in radians (left negative, right positive).";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new BodySwirlDetectorConfiguration();
    }
}
