using System;
using System.Composition;

namespace OpenSense.Components.GoogleCloud.Speech.V1 {
    [Export(typeof(IComponentMetadata))]
    public class GoogleCloudSpeechMetadata : ConventionalComponentMetadata {

        public override string Description => "Google Cloud Speech V1 Recognizer. Requires credential file from Google. Only accepts 16bit PCM.";

        protected override Type ComponentType => typeof(GoogleCloudSpeech);

        public override string Name => "Google Cloud Speech Recognizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(GoogleCloudSpeech.In):
                    return "[Required] Audio signal paired with voice activity detection result.";
                case nameof(GoogleCloudSpeech.Out):
                    return "Speech recognition results.";
                case nameof(GoogleCloudSpeech.AudioOut):
                    return "For debugging, the portion of input audio signal that was send to Google server.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new GoogleCloudSpeechConfiguration();
    }
}
