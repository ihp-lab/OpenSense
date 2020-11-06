using System;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Serializable]
    public class WaveFileWriterConfiguration : ConventionalComponentConfiguration {

        private string filename = "audio.wav";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        public override IComponentMetadata GetMetadata() => new WaveFileWriterMetadata();

        protected override object Instantiate(Pipeline pipeline) => new WaveFileWriter(pipeline, Filename);
    }
}
