using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.BodyGestureDetectors {
    [Serializable]
    public class BodySwirlDetectorConfiguration : ConventionalComponentConfiguration {

        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private float outputOffset = 0;

        public float OutputOffset {
            get => outputOffset;
            set => SetProperty(ref outputOffset, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        public DeliveryPolicy DefaultDeliveryPolicy { get; set; } = null;

        public DeliveryPolicy BodyTrackerDeliveryPolicy { get; set; } = null;

        public override IComponentMetadata GetMetadata() => new BodySwirlDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BodySwirlDetector(pipeline) {
            BodyIndex = BodyIndex,
            MinimumConfidenceLevel = MinimumConfidenceLevel,
            OutputOffset = OutputOffset,
        };
    }
}
