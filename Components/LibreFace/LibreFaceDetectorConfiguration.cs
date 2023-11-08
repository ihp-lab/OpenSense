using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace {
    [Serializable]
    public class LibreFaceDetectorConfiguration : ConventionalComponentConfiguration {

        #region Settings
        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        private bool inferenceActionUnitIntensity = true;

        public bool InferenceActionUnitIntensity {
            get => inferenceActionUnitIntensity;
            set => SetProperty(ref inferenceActionUnitIntensity, value);
        }

        private bool inferenceActionUnitPresence = true;

        public bool InferenceActionUnitPresence {
            get => inferenceActionUnitPresence;
            set => SetProperty(ref inferenceActionUnitPresence, value);
        }

        private bool inferenceFacialExpression = true;

        public bool InferenceFacialExpression {
            get => inferenceFacialExpression;
            set => SetProperty(ref inferenceFacialExpression, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new LibreFaceDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider? serviceProvider) => 
            new LibreFaceDetector(pipeline, DeliveryPolicy, InferenceActionUnitIntensity, InferenceActionUnitPresence, InferenceFacialExpression) {
                Logger = serviceProvider?.GetService(typeof(ILogger)) as ILogger,
            };
    }
}
