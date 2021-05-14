using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.GoogleCloud.Speech.V1 {
    [Export(typeof(IComponentMetadata))]
    public class GoogleCloudSpeechMetadata : ConventionalComponentMetadata {

        public override string Description => "Google cloud speech recognizer.";

        protected override Type ComponentType => typeof(GoogleCloudSpeech);

        public override ComponentConfiguration CreateConfiguration() => new GoogleCloudSpeechConfiguration();
    }
}
