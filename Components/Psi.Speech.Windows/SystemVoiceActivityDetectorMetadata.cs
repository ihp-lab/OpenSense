using System;
using System.Composition;
using Microsoft.Psi.Speech;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Speech {
    [Export(typeof(IComponentMetadata))]
    public class SystemVoiceActivityDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Windows built-in voice activity detector.";

        protected override Type ComponentType => typeof(SystemVoiceActivityDetector);

        public override string Name => "System Voice Activity Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(SystemVoiceActivityDetector.In):
                    return "[Required] Audio signal.";
                case nameof(SystemVoiceActivityDetector.Out):
                    return "Booleans indicating voice activity.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new SystemVoiceActivityDetectorConfiguration();
    }
}
