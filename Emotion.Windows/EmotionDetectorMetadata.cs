using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Emotion {
    [Export(typeof(IComponentMetadata))]
    public class EmotionDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect emotions.";

        protected override Type ComponentType => typeof(EmotionDetector);

        public override ComponentConfiguration CreateConfiguration() => new EmotionDetectorConfiguration();
    }
}
