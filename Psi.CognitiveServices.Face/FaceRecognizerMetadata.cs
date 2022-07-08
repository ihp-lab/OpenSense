using System;
using System.Composition;
using Microsoft.Psi.CognitiveServices.Face;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.CognitiveServices.Face {
    [Export(typeof(IComponentMetadata))]
    public class FaceRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Azure Face Recognition. Requires a subscription key.";

        protected override Type ComponentType => typeof(FaceRecognizer);

        public override string Name => "Azure Face Recognizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FaceRecognizer.In):
                    return "[Required] Images.";
                case nameof(FaceRecognizer.Out):
                    return "Recognized identities.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FaceRecognizerConfiguration();
    }
}
