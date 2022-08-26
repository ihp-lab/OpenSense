using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.BodyGestureDetectors {
    [Export(typeof(IComponentMetadata))]
    public class ArmsProximityDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detects distance between corssed arms. Requires Azure Kinect outputs.";

        protected override Type ComponentType => typeof(ArmsProximityDetector);

        public override string Name => "Arms Proximity Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ArmsProximityDetector.In):
                    return "[Required] Tracked bodies of Azure Kinect.";
                case nameof(ArmsProximityDetector.Out):
                    return "Floating point shortest distance between arms measured in meters.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ArmsProximityDetectorConfiguration();
    }
}
