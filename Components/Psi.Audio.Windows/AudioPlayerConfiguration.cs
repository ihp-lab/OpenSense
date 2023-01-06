using System;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Audio {
    [Serializable]
    public class AudioPlayerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Audio.AudioPlayerConfiguration raw = new Microsoft.Psi.Audio.AudioPlayerConfiguration();

        public Microsoft.Psi.Audio.AudioPlayerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new AudioPlayerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioPlayer(pipeline, Raw);
    }
}
