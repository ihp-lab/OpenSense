using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace {
    [Export(typeof(IComponentMetadata))]
    public class LibreFaceDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect Action Unit intensities and facial expressions. Requires MediaPipe face landmark detection results. The included AUs are 1, 2, 4, 5, 6, 9, 12, 15, 17, 20, 25, 26.";

        protected override Type ComponentType => typeof(LibreFaceDetector);

        public override string Name => "LibreFace";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(LibreFaceDetector.DataIn):
                    return "[Required] Face landmark detection results from MediaPipe.";
                case nameof(LibreFaceDetector.ImageIn):
                    return "[Required] Images. Same as those were sent to MediaPipe.";
                case nameof(LibreFaceDetector.ActionUnitIntensityOut):
                    return "A list of Action Unit intensities of detected faces. Range 0 - 5.";
                case nameof(LibreFaceDetector.ActionUnitPresenceOut):
                    return "A list of Action Unit presences of detected faces. Range false or true.";
                case nameof(LibreFaceDetector.FacialExpressionOut):
                    return "A list of facial expressions of detected faces. Range 0 - 1.";
                case nameof(LibreFaceDetector.AlignedImagesOut):
                    return "A list of aligned face images. For debug purpose.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new LibreFaceDetectorConfiguration();
    }
}
