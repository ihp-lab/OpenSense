using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;
using OpenSense.Component.EyePointOfInterest.Common;

namespace OpenSense.Component.EyePointOfInterest {
    [Serializable]
    public class DisplayPoiEstimatorConfiguration : ConventionalComponentConfiguration {

        private string estimatorConfigurationFilename;

        public string EstimatorConfigurationFilename {
            get => estimatorConfigurationFilename;
            set => SetProperty(ref estimatorConfigurationFilename, value);
        }

        public override IComponentMetadata GetMetadata() => new DisplayPoiEstimatorMetadata();

        protected override object Instantiate(Pipeline pipeline) => new DisplayPoiEstimator(pipeline) { 
            Estimator = string.IsNullOrEmpty(EstimatorConfigurationFilename) ? null : PoiOnDisplayEstimatorHelper.LoadEstimator(EstimatorConfigurationFilename),
        };
    }
}
