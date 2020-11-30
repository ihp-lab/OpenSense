using System;
using OpenSense.Component.EyePointOfInterest.Common;

namespace OpenSense.Component.EyePointOfInterest.SpatialTracking {
    [Serializable]
    public class SpatialTrackingPoiOnDisplayEstimatorConfiguration : PoiOnDisplayEstimatorConfiguration {
        public int Order { get; set; }
        public GazeToDisplayCoordinateMappingRecord[] Samples { get; set; }

        public override Type ConfigurationType => typeof(SpatialTrackingPoiOnDisplayEstimatorConfiguration);

        public override IPoiOnDisplayEstimator Instantiate() => new SpatialTrackingPoiOnDisplayEstimator(this);
    }
}
