using System;
using Microsoft.Psi;
using Microsoft.Psi.CognitiveServices.Speech;

namespace OpenSense.Components.Psi.CognitiveServices.Speech {
    [Serializable]
    public class AzureSpeechRecognizerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration raw = new Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration();

        public Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new AzureSpeechRecognizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AzureSpeechRecognizer(pipeline, Raw);
    }
}
