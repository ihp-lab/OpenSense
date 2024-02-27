using System;

namespace OpenSense.Components.EyePointOfInterest {
    [Serializable]
    public abstract class PoiOnDisplayEstimatorConfiguration {

        public const string TypeDiscriminatorPropertyName = "$configuration-type";

        public abstract IPoiOnDisplayEstimator Instantiate();
    }
}
