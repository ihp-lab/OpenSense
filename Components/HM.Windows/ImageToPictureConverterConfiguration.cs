using System;
using HMInterop;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public class ImageToPictureConverterConfiguration : ConventionalComponentConfiguration {

        private ChromaFormat chromaFormat = ChromaFormat.Chroma400;

        public ChromaFormat ChromaFormat {
            get => chromaFormat;
            set => SetProperty(ref chromaFormat, value);
        }

        private int outputBitDepth = 16;

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
