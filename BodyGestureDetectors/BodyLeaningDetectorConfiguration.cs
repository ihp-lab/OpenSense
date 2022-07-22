using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.BodyGestureDetectors {
    [Serializable]
    public class BodyLeaningDetectorConfiguration : ConventionalComponentConfiguration {

        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        public DeliveryPolicy DefaultDeliveryPolicy { get; set; } = null;

        public DeliveryPolicy BodyTrackerDeliveryPolicy { get; set; } = null;

        public override IComponentMetadata GetMetadata() => new BodyLeaningDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BodyLeaningDetector(pipeline) { 
            BodyIndex = BodyIndex
        };
    }
}
