using System;

namespace OpenSense.Component.EyePointOfInterest.Common {
    [Serializable]
    public abstract class PoiOnDisplayEstimatorConfiguration {

        public abstract Type ConfigurationType { get; }

        public abstract IPoiOnDisplayEstimator Instantiate();
    }
}
