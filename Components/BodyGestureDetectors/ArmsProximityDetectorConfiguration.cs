using System;
using Microsoft.Psi;

namespace OpenSense.Components.BodyGestureDetectors {
    [Serializable]
    public class ArmsProximityDetectorConfiguration : ConventionalComponentConfiguration {

        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Low;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        private double invalidValue = double.NaN;

        public double InvalidValue {
            get => invalidValue;
            set => SetProperty(ref invalidValue, value);
        }

        private bool postInvalidOnArmsNotDetected = true;

        public bool PostInvalidOnArmsNotDetected {
            get => postInvalidOnArmsNotDetected;
            set => SetProperty(ref postInvalidOnArmsNotDetected, value);
        }

        private bool postInvalidOnArmsNotOverlapped = true;

        public bool PostInvalidOnArmsNotOverlapped {
            get => postInvalidOnArmsNotOverlapped;
            set => SetProperty(ref postInvalidOnArmsNotOverlapped, value);
        }

        public override IComponentMetadata GetMetadata() => new ArmsProximityDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ArmsProximityDetector(pipeline) {
            BodyIndex = BodyIndex,
            MinimumConfidenceLevel = MinimumConfidenceLevel,
        };
    }
}
