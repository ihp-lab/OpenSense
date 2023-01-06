using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.BodyGestureDetectors {
    [Serializable]
    public class BodyAttitudeDetectorConfiguration : ConventionalComponentConfiguration {

        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private float yawOffset = 0;

        public float YawOffset {
            get => yawOffset;
            set => SetProperty(ref yawOffset, value);
        }

        private float pitchOffset = 0;

        public float PitchOffset {
            get => pitchOffset;
            set => SetProperty(ref pitchOffset, value);
        }

        private float rollOffset = 0;

        public float RollOffset {
            get => rollOffset;
            set => SetProperty(ref rollOffset, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        public override IComponentMetadata GetMetadata() => new BodyAttitudeDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BodyAttitudeDetector(pipeline) { 
            BodyIndex = BodyIndex,
            MinimumConfidenceLevel = MinimumConfidenceLevel,
            YawOffset = YawOffset,
            PitchOffset = PitchOffset,
            RollOffset = RollOffset,
        };
    }
}
