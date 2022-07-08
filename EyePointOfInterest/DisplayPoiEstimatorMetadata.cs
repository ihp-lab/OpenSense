using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.EyePointOfInterest {
    [Export(typeof(IComponentMetadata))]
    public class DisplayPoiEstimatorMetadata : ConventionalComponentMetadata {

        public override string Description => "Estimate point of interest on a display based on gaze infomation. Requires OpenFace outputs and a regressor parameter file.";

        protected override Type ComponentType => typeof(DisplayPoiEstimator);

        public override string Name => "Display POI Estimator";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(DisplayPoiEstimator.In):
                    return "[Required] OpenFace outputs for getting head transforms.";
                case nameof(DisplayPoiEstimator.Out):
                    return "Estimated normalized 2D point of interest results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new DisplayPoiEstimatorConfiguration();
    }
}
