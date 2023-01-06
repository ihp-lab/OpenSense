using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioPlayerMetadata : ConventionalComponentMetadata {

        public override string Description => "Playbacks audio signal to a speaker.";

        protected override Type ComponentType => typeof(AudioPlayer);

        public override string Name => "Audio Player";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AudioPlayer.In):
                    return "[Required] Audio signal.";
                case nameof(AudioPlayer.AudioLevelInput):
                    return "[Optional] Audio level.";
                case nameof(AudioPlayer.AudioLevel):
                    return "Audio level.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AudioPlayerConfiguration();
    }
}
