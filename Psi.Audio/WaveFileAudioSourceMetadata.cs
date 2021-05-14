using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class WaveFileAudioSourceMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that streams audio from a WAVE file.";

        protected override Type ComponentType => typeof(WaveFileAudioSource);

        public override ComponentConfiguration CreateConfiguration() => new WaveFileAudioSourceConfiguration();
    }
}
