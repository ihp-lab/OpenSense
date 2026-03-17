using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class PictureToDepthImageConverterConfiguration : ConventionalComponentConfiguration {

        #region Input
        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
        #endregion

        #region Depth Metadata
        private DepthValueSemantics depthValueSemantics = DepthValueSemantics.DistanceToPlane;

        public DepthValueSemantics DepthValueSemantics {
            get => depthValueSemantics;
            set => SetProperty(ref depthValueSemantics, value);
        }

        private double depthValueToMetersScaleFactor = 0.001;

        public double DepthValueToMetersScaleFactor {
            get => depthValueToMetersScaleFactor;
            set => SetProperty(ref depthValueToMetersScaleFactor, value);
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

        public override IComponentMetadata GetMetadata() => new PictureToDepthImageConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new PictureToDepthImageConverter(pipeline) {
            InputBitDepth = InputBitDepth,
            DepthValueSemantics = DepthValueSemantics,
            DepthValueToMetersScaleFactor = DepthValueToMetersScaleFactor,
            BitDepthMappingEnabled = BitDepthMappingEnabled,
            BitDepthMappingScaleShift = BitDepthMappingScaleShift,
            BitDepthMappingInputStart = BitDepthMappingInputStart,
            BitDepthMappingOutputStart = BitDepthMappingOutputStart,
        };
    }
}
