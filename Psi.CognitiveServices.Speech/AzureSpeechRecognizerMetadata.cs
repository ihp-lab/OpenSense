using System;
using System.Composition;
using Microsoft.Psi.CognitiveServices.Speech;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.CognitiveServices.Speech {
    [Export(typeof(IComponentMetadata))]
    public class AzureSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that performs speech recognition using the Microsoft Cognitive Services Azure Speech API.";

        protected override Type ComponentType => typeof(AzureSpeechRecognizer);

        public override ComponentConfiguration CreateConfiguration() => new AzureSpeechRecognizerConfiguration();
    }
}
