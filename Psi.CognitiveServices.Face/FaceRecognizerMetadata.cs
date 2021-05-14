using System;
using System.Composition;
using Microsoft.Psi.CognitiveServices.Face;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.CognitiveServices.Face {
    [Export(typeof(IComponentMetadata))]
    public class FaceRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that performs face recognition via Microsoft Cognitive Services Face API.";

        protected override Type ComponentType => typeof(FaceRecognizer);

        public override ComponentConfiguration CreateConfiguration() => new FaceRecognizerConfiguration();
    }
}
