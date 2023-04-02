using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.PortableFACS {
    [Export(typeof(IComponentMetadata))]
    public class ActionUnitDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect Action Unit intensities. Requires MediaPipe face landmark detection results. The included AUs are 1, 2, 4, 5, 6, 9, 12, 15, 17, 20, 25, 26.";

        protected override Type ComponentType => typeof(ActionUnitDetector);

        public override string Name => "Action Unit Detector";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(ActionUnitDetector.DataIn):
                    return "[Required] Face landmark detection results from MediaPipe.";
                case nameof(ActionUnitDetector.ImageIn):
                    return "[Required] Images. Same as those were sent to MediaPipe.";
                case nameof(ActionUnitDetector.Out):
                    return "A list of Action Units of detected faces.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new ActionUnitDetectorConfiguration();
    }
}
