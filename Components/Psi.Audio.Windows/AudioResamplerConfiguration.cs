using System;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.Psi.Audio {
    [Serializable]
    public class AudioResamplerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Audio.AudioResamplerConfiguration raw = new Microsoft.Psi.Audio.AudioResamplerConfiguration();

        public Microsoft.Psi.Audio.AudioResamplerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new AudioResamplerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioResampler(pipeline, Raw);
    }
}
