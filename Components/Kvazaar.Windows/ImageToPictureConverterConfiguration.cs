using System;
using KvazaarInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    [Serializable]
    public class ImageToPictureConverterConfiguration : ConventionalComponentConfiguration {

        #region Input
        private PixelFormat? inputPixelFormat;

        public PixelFormat? InputPixelFormat {
            get => inputPixelFormat;
            set => SetProperty(ref inputPixelFormat, value);
        }

        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
        #endregion

        #region Output
#if FIXED_BIT_DEPTH
        public int OutputBitDepth => Picture.MaxBitDepth;
#else
        private int outputBitDepth = 8;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }
#endif

        private ChromaFormat outputChromaFormat = ChromaFormat.Csp400;

        public ChromaFormat OutputChromaFormat {
            get => outputChromaFormat;
            set => SetProperty(ref outputChromaFormat, value);
        }
        #endregion

        #region Chroma Conversion
        private bool chromaConvertEnabled;

        public bool ChromaConvertEnabled {
            get => chromaConvertEnabled;
            set => SetProperty(ref chromaConvertEnabled, value);
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

        private int bitDepthMappingInputStart;

        public int BitDepthMappingInputStart {
            get => bitDepthMappingInputStart;
            set => SetProperty(ref bitDepthMappingInputStart, value);
        }

        private int bitDepthMappingOutputStart;

        public int BitDepthMappingOutputStart {
            get => bitDepthMappingOutputStart;
            set => SetProperty(ref bitDepthMappingOutputStart, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new ImageToPictureConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageToPictureConverter(pipeline) {
            InputPixelFormat = InputPixelFormat,
            InputBitDepth = InputBitDepth,
#if !FIXED_BIT_DEPTH
            OutputBitDepth = OutputBitDepth,
#endif
            OutputChromaFormat = OutputChromaFormat,
            ChromaConvertEnabled = ChromaConvertEnabled,
            BitDepthMappingEnabled = BitDepthMappingEnabled,
            BitDepthMappingScaleShift = BitDepthMappingScaleShift,
            BitDepthMappingInputStart = BitDepthMappingInputStart,
            BitDepthMappingOutputStart = BitDepthMappingOutputStart,
        };
    }
}
