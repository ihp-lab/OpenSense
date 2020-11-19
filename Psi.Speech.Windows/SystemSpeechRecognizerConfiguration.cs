using System;
using Microsoft.Psi;
using Microsoft.Psi.Speech;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Speech {
    [Serializable]
    public class SystemSpeechRecognizerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration raw = new Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration();

        public Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new SystemSpeechRecognizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new SystemSpeechRecognizer(pipeline, Raw);
    }
}
