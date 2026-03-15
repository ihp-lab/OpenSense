using System;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class ImageToPictureConverterConfiguration : ConventionalComponentConfiguration {

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
        private int outputBitDepth = 16;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
        }

        private ChromaFormat outputChromaFormat = ChromaFormat.Chroma400;

        public ChromaFormat OutputChromaFormat {
            get => outputChromaFormat;
            set => SetProperty(ref outputChromaFormat, value);
        }
        #endregion

        #region Bit Depth Mapping
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
        #endregion

        public override IComponentMetadata GetMetadata() => new ImageToPictureConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageToPictureConverter(pipeline) {
            InputPixelFormat = InputPixelFormat,
            InputBitDepth = InputBitDepth,
            OutputBitDepth = OutputBitDepth,
            OutputChromaFormat = OutputChromaFormat,
            BitDepthMappingScaleShift = BitDepthMappingScaleShift,
            BitDepthMappingWindow = BitDepthMappingWindow,
        };
    }
}
