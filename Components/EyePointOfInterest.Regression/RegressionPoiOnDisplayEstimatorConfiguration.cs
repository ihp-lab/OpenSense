namespace OpenSense.Components.EyePointOfInterest.Regression {
    public class RegressionPoiOnDisplayEstimatorConfiguration : PoiOnDisplayEstimatorConfiguration {

        public byte[] PredictorX { get; set; }

        public byte[] PredictorY { get; set; }

        public override IPoiOnDisplayEstimator Instantiate() => new RegressionPoiOnDisplayEstimator(this);
    }
}
