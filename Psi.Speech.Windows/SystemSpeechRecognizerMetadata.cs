using System;
using System.Composition;
using Microsoft.Psi.Speech;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Speech {
    [Export(typeof(IComponentMetadata))]
    public class SystemSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that performs speech recognition using the desktop speech recognition engine from `System.Speech`.";

        protected override Type ComponentType => typeof(SystemSpeechRecognizer);

        public override ComponentConfiguration CreateConfiguration() => new SystemSpeechRecognizerConfiguration();
    }
}
