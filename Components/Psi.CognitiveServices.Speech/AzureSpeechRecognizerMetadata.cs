using System;
using System.Composition;
using Microsoft.Psi.CognitiveServices.Speech;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.CognitiveServices.Speech {
    [Export(typeof(IComponentMetadata))]
    public class AzureSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Azure Speech Recognition. Requires a subscription key.";

        protected override Type ComponentType => typeof(AzureSpeechRecognizer);

        public override string Name => "Azure Speech Recognizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureSpeechRecognizer.In):
                    return "[Required] Audio signal paired with voice activity detection result.";
                case nameof(AzureSpeechRecognizer.Out):
                    return "Final recognition results.";
                case nameof(AzureSpeechRecognizer.PartialRecognitionResults):
                    return "Partial recognition results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureSpeechRecognizerConfiguration();
    }
}
