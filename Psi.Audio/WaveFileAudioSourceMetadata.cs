using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class WaveFileAudioSourceMetadata : ConventionalComponentMetadata {

        public override string Description => "Read audio signal from a WAVE file.";

        protected override Type ComponentType => typeof(WaveFileAudioSource);

        public override string Name => "Wave File Audio Source";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(WaveFileAudioSource.Out):
                    return "Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new WaveFileAudioSourceConfiguration();
    }
}
