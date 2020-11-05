using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioPlayerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that plays back an audio stream to an output device such as the speakers.";

        protected override Type ComponentType => typeof(AudioPlayer);

        public override ComponentConfiguration CreateConfiguration() => new AudioPlayerConfiguration();
    }
}
