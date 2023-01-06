using System;
using Microsoft.Psi;
using Microsoft.Psi.CognitiveServices.Face;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.CognitiveServices.Face {
    [Serializable]
    public class FaceRecognizerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.CognitiveServices.Face.FaceRecognizerConfiguration raw = new Microsoft.Psi.CognitiveServices.Face.FaceRecognizerConfiguration(
                "",
                "",
                ""
            );

        public Microsoft.Psi.CognitiveServices.Face.FaceRecognizerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new FaceRecognizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FaceRecognizer(pipeline, Raw);
    }
}
