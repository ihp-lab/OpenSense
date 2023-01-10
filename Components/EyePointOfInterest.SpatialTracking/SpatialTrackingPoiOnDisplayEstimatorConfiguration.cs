using System;

namespace OpenSense.Components.EyePointOfInterest.SpatialTracking {
    [Serializable]
    public class SpatialTrackingPoiOnDisplayEstimatorConfiguration : PoiOnDisplayEstimatorConfiguration {
        public int Order { get; set; }
        public GazeToDisplayCoordinateMappingRecord[] Samples { get; set; }

        public override Type ConfigurationType => typeof(SpatialTrackingPoiOnDisplayEstimatorConfiguration);

        public override IPoiOnDisplayEstimator Instantiate() => new SpatialTrackingPoiOnDisplayEstimator(this);
    }
}
