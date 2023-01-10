using System;

namespace OpenSense.Components.EyePointOfInterest {
    [Serializable]
    public abstract class PoiOnDisplayEstimatorConfiguration {

        public abstract Type ConfigurationType { get; }

        public abstract IPoiOnDisplayEstimator Instantiate();
    }
}
