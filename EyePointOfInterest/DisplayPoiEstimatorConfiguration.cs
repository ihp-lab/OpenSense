using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;
using OpenSense.Components.EyePointOfInterest.Common;

namespace OpenSense.Components.EyePointOfInterest {
    [Serializable]
    public class DisplayPoiEstimatorConfiguration : ConventionalComponentConfiguration {

        private string estimatorConfigurationFilename;

        public string EstimatorConfigurationFilename {
            get => estimatorConfigurationFilename;
            set => SetProperty(ref estimatorConfigurationFilename, value);
        }

        public override IComponentMetadata GetMetadata() => new DisplayPoiEstimatorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DisplayPoiEstimator(pipeline) { 
            Estimator = string.IsNullOrEmpty(EstimatorConfigurationFilename) ? null : PoiOnDisplayEstimatorHelper.LoadEstimator(EstimatorConfigurationFilename),
        };
    }
}
