using System;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace {
    [Serializable]
    public class LibreFaceDetectorConfiguration : ConventionalComponentConfiguration {

        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        public override IComponentMetadata GetMetadata() => new LibreFaceDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new LibreFaceDetector(pipeline, DeliveryPolicy) {
            
        };
    }
}
