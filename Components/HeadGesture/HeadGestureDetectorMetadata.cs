using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.HeadGesture {
    [Export(typeof(IComponentMetadata))]
    public class HeadGestureDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect head gesture (Nod, Shake or Tilt). Requires outputs from OpenFace.";

        protected override Type ComponentType => typeof(HeadGestureDetector);

        public override string Name => "Head Gesture Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(HeadGestureDetector.In):
                    return "[Required] OpenFace outputs used for getting head transform.";
                case nameof(HeadGestureDetector.Out):
                    return "Detected head gesture. A enum type.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new HeadGestureDetectorConfiguration();
    }
}
