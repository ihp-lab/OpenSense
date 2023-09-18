using System;
using System.Composition;
using Microsoft.Psi.Speech;

namespace OpenSense.Components.Psi.Speech {
    [Export(typeof(IComponentMetadata))]
    public class SystemSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Windows built-in speech recognizer.";

        protected override Type ComponentType => typeof(SystemSpeechRecognizer);

        public override string Name => "System Speech Recognizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(SystemSpeechRecognizer.In):
                    return "[Required] Audio signal.";
                case nameof(SystemSpeechRecognizer.ReceiveGrammars):
                    return "[Optional] Update grammars set to the recognizer.";
                case nameof(SystemSpeechRecognizer.Out):
                    return "Recognition results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new SystemSpeechRecognizerConfiguration();
    }
}
