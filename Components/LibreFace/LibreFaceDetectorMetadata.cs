using System;
using System.Composition;

namespace OpenSense.Components.LibreFace {
    [Export(typeof(IComponentMetadata))]
    public class LibreFaceDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => 
            "Detect Action Unit intensities, precenses and facial expressions. Requires MediaPipe face landmark detection results."
            + " The included AUs are 1, 2, 4, 5(I), 6, 7(P), 9(I), 10(P), 12, 14(P), 15, 17, 20(I), 23(P), 24(P), 25(I), 26(I). Where \"(I)\" denotes intensity-only; \"(P)\" denotes persence-only."
            + " Expressions are Neutral, Happiness, Sadness, Surprise, Fear, Disgust, Anger, Contempt."
            + " CUDA acceleration is enabled through ONNX. ONNX Runtime 1.20.1 is used."
            + " Please refer to the ONNX Runtime Requirements page for setup instructions, CUDA and cuDNN version requirements."
            ;

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
