using System;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Serializable]
    public class Mpeg4WriterConfiguration : ConventionalComponentConfiguration {

        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private Microsoft.Psi.Media.Mpeg4WriterConfiguration raw = Microsoft.Psi.Media.Mpeg4WriterConfiguration.Default;

        public Microsoft.Psi.Media.Mpeg4WriterConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new Mpeg4WriterMetadata();

        protected override object Instantiate(Microsoft.Psi.Pipeline pipeline) => new Mpeg4Writer(pipeline, Filename, Raw);
    }
}
