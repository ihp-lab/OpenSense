using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioCaptureMetadata : ConventionalComponentMetadata {

        public override string Description => "Captures audio from a microphone.";

        protected override Type ComponentType => typeof(AudioCapture);

        public override string Name => "Audio Capture";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AudioCapture.AudioLevelInput):
                    return "[Optional] Controls audio level.";
                case nameof(AudioCapture.Out):
                    return "Audio signal captured from the selected microphone.";
                case nameof(AudioCapture.AudioLevel):
                    return "Audio level";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AudioCaptureConfiguration();
    }
}
