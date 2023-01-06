using System;
using OpenSense.Components.EyePointOfInterest.Common;

namespace OpenSense.Components.EyePointOfInterest.Regression {
    public class RegressionPoiOnDisplayEstimatorConfiguration : PoiOnDisplayEstimatorConfiguration {

        public byte[] PredictorX { get; set; }
        public byte[] PredictorY { get; set; }

        public override Type ConfigurationType => typeof(RegressionPoiOnDisplayEstimatorConfiguration);

        public override IPoiOnDisplayEstimator Instantiate() => new RegressionPoiOnDisplayEstimator(this);
    }
}
