using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Whisper.NET {
    [Export(typeof(IComponentMetadata))]
    public class WhisperMetadata : ConventionalComponentMetadata {

        public override string Description => "[Experimental] Local deploy of OpenAI Whisper Speech Recognizer. Please use 16kHz 16-bit PCM. Channels will be averaged. Model file will be downloaded if not found. Limitations: no partial results; no alternatives; need VAD to aid audio segregation; audio is buffered when VAD is active; not all options are exposed.";

        protected override Type ComponentType => typeof(WhisperProcessor);

        public override string Name => "Whisper";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(WhisperProcessor.In):
                    return "[Required] Audio signal paired with voice activity detection result.";
                case nameof(WhisperProcessor.Out):
                    return "Speech recognition results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new WhisperConfiguration();
    }
}
