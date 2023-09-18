using System;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.Psi.Audio {
    [Serializable]
    public class WaveFileWriterConfiguration : ConventionalComponentConfiguration {

        private string filename = "audio.wav";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        public override IComponentMetadata GetMetadata() => new WaveFileWriterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new WaveFileWriter(pipeline, Filename);
    }
}
