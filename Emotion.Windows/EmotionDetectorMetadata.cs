using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Emotion {
    [Export(typeof(IComponentMetadata))]
    public class EmotionDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect emotions. Not very accurate. Requires outputs from OpenFace.";

        protected override Type ComponentType => typeof(EmotionDetector);

        public override string Name => "Emotion Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(EmotionDetector.ImageIn):
                    return "[Required] Same images that were sent to OpenFace.";
                case nameof(EmotionDetector.HeadPoseIn):
                    return "[Required] OpenFace outputs for knwoing head bounding box and do the clipping.";
                case nameof(EmotionDetector.Out):
                    return "[Composite] Magnitude of certain kinds of emotions.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new EmotionDetectorConfiguration();
    }
}
