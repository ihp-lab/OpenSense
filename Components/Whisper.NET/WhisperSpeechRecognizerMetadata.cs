using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Whisper.NET {
    [Export(typeof(IComponentMetadata))]
    public sealed class WhisperSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description => "[Experimental] Local deploy of OpenAI Whisper Speech Recognizer. Please use 16kHz 16-bit PCM. Channels will be averaged. Model file will be downloaded if not found. There will be no alternative results.";

        protected override Type ComponentType => typeof(WhisperSpeechRecognizer);

        public override string Name => "Whisper";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(WhisperSpeechRecognizer.In):
                    return "[Required] Audio signal paired with voice activity detection result.";
                case nameof(WhisperSpeechRecognizer.PartialOut):
                    return "Partial speech recognition results.";
                case nameof(WhisperSpeechRecognizer.FinalOut):
                    return "Final speech recognition results.";
                case nameof(WhisperSpeechRecognizer.Out):
                    return "Speech recognition results. Both partial and final.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new WhisperSpeechRecognizerConfiguration();
    }
}
