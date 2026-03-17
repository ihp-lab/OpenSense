using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class DepthImageToPictureConverterConfiguration : ConventionalComponentConfiguration {

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

        #region Output
        private int outputBitDepth = 16;

        public int OutputBitDepth {
            get => outputBitDepth;
            set => SetProperty(ref outputBitDepth, value);
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

        public override IComponentMetadata GetMetadata() => new DepthImageToPictureConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DepthImageToPictureConverter(pipeline) {
            DepthValueSemantics = DepthValueSemantics,
            DepthValueToMetersScaleFactor = DepthValueToMetersScaleFactor,
            OutputBitDepth = OutputBitDepth,
            BitDepthMappingEnabled = BitDepthMappingEnabled,
            BitDepthMappingScaleShift = BitDepthMappingScaleShift,
            BitDepthMappingInputStart = BitDepthMappingInputStart,
            BitDepthMappingOutputStart = BitDepthMappingOutputStart,
        };
    }
}
