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


        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        /* Torso */

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

        /* Head */
        private float headYawOffset = 0;

        public float HeadYawOffset {
            get => headYawOffset;
            set => SetProperty(ref headYawOffset, value);
        }

        private float headPitchOffset = 0;

        public float HeadYPitchOffset {
            get => headPitchOffset;
            set => SetProperty(ref headPitchOffset, value);
        }

        private float headRollOffset = 0;

        public float HeadRollOffset {
            get => headRollOffset;
            set => SetProperty(ref headRollOffset, value);
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
