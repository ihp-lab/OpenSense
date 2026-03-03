using System;
using KvazaarInterop;
using Microsoft.Psi;

namespace OpenSense.Components.Kvazaar {
    [Serializable]
    public class ImageToPictureConverterConfiguration : ConventionalComponentConfiguration {

        private ChromaFormat chromaFormat = ChromaFormat.Csp400;

        public ChromaFormat ChromaFormat {
            get => chromaFormat;
            set => SetProperty(ref chromaFormat, value);
        }

        private int outputBitDepth = 8;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }

        public override IComponentMetadata GetMetadata() => new ImageToPictureConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageToPictureConverter(pipeline) {
            ChromaFormat = ChromaFormat,
            OutputBitDepth = OutputBitDepth,
        };
    }
}
