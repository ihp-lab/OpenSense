using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class PictureToImageConverterConfiguration : ConventionalComponentConfiguration {

        #region Output
        private PixelFormat outputPixelFormat = PixelFormat.Gray_16bpp;

        public PixelFormat OutputPixelFormat {
            get => outputPixelFormat;
            set => SetProperty(ref outputPixelFormat, value);
        }
        #endregion

        #region Chroma Conversion
        private bool chromaConvertEnabled;

        public bool ChromaConvertEnabled {
            get => chromaConvertEnabled;
            set => SetProperty(ref chromaConvertEnabled, value);
        }

        private ChromaUpsampleMethod chromaUpsampleMethod = ChromaUpsampleMethod.NearestNeighbor;

        public ChromaUpsampleMethod ChromaUpsampleMethod {
            get => chromaUpsampleMethod;
            set => SetProperty(ref chromaUpsampleMethod, value);
        }
        #endregion

        #region Bit Depth Mapping
        private bool bitDepthMappingEnabled;

        public bool BitDepthMappingEnabled {
            get => bitDepthMappingEnabled;
            set => SetProperty(ref bitDepthMappingEnabled, value);
        }

        private int bitDepthMappingScaleShift;

        public int BitDepthMappingScaleShift {
            get => bitDepthMappingScaleShift;
            set => SetProperty(ref bitDepthMappingScaleShift, value);
        }

        private int bitDepthMappingWindow;

        public int BitDepthMappingWindow {
            get => bitDepthMappingWindow;
            set => SetProperty(ref bitDepthMappingWindow, value);
        }

        private int sourceBitDepth;

        public int SourceBitDepth {
            get => sourceBitDepth;
            set => SetProperty(ref sourceBitDepth, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new PictureToImageConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new PictureToImageConverter(pipeline) {
            OutputPixelFormat = OutputPixelFormat,
            ChromaConvertEnabled = ChromaConvertEnabled,
            ChromaUpsampleMethod = ChromaUpsampleMethod,
            BitDepthMappingEnabled = BitDepthMappingEnabled,
            BitDepthMappingScaleShift = BitDepthMappingScaleShift,
            BitDepthMappingWindow = BitDepthMappingWindow,
            SourceBitDepth = SourceBitDepth,
        };
    }
}
