using System;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.Psi.Audio {
    [Serializable]
    public class AudioCaptureConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Audio.AudioCaptureConfiguration raw = new Microsoft.Psi.Audio.AudioCaptureConfiguration();

        public Microsoft.Psi.Audio.AudioCaptureConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new AudioCaptureMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioCapture(pipeline, Raw);
    }
}
