using System;

namespace OpenSense.Components.EyePointOfInterest.SpatialTracking {
    [Serializable]
    public sealed class SpatialTrackingPoiOnDisplayEstimatorConfiguration : PoiOnDisplayEstimatorConfiguration {
        public int Order { get; set; }

        public GazeToDisplayCoordinateMappingRecord[] Samples { get; set; }

        public override IPoiOnDisplayEstimator Instantiate() => new SpatialTrackingPoiOnDisplayEstimator(this);
    }
}
