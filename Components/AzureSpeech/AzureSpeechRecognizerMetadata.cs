using System.Composition;

namespace OpenSense.Components.AzureSpeech {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureSpeechRecognizerMetadata : ConventionalComponentMetadata {

        public override string Description =>
            "Azure cloud AI speech recognizer." 
            + " This recognizer aims to provide rich features and replace the recognizer implemented by Microsoft \\psi." 
            + " Requires a subscription key. Only 16kHz 16bit Mono PCM is supported."
            + " On Windows, 64-bit target architecture and Microsoft Visual C++ Redistributable are required."
            + " Remember to use the Unlimited delivery policy to get correct results."
            ;

        protected override Type ComponentType => typeof(AzureSpeechRecognizer);

        public override string Name => "Azure Speech Recognizer";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AzureSpeechRecognizer.In):
                    return "[Required] Audio signal paired with voice activity detection result. Only 16kHz 16bit Mono PCM is supported.";
                case nameof(AzureSpeechRecognizer.PartialOut):
                    return "Partial speech recognition results.";
                case nameof(AzureSpeechRecognizer.FinalOut):
                    return "Final speech recognition results.";
                case nameof(AzureSpeechRecognizer.Out):
                    return "Speech recognition results. Both partial and final.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureSpeechRecognizerConfiguration();
    }
}
