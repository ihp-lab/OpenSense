using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.EyePointOfInterest {
    [Export(typeof(IComponentMetadata))]
    public class DisplayPoiEstimatorMetadata : ConventionalComponentMetadata {

        public override string Description => "Estimate point of interest on display based on gaze infomation.";

        protected override Type ComponentType => typeof(DisplayPoiEstimator);

        public override ComponentConfiguration CreateConfiguration() => new DisplayPoiEstimatorConfiguration();
    }
}
