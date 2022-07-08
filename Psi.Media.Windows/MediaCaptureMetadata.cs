using System;
using System.Composition;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class MediaCaptureMetadata : ConventionalComponentMetadata {

        public override string Description => "Captures video and audio from a camera.";

        protected override Type ComponentType => typeof(MediaCapture);

        public override string Name => "Media Capture";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(MediaCapture.Out):
                    return "Captured images. Same as the Video port.";
                case nameof(MediaCapture.Video):
                    return "Captured images. Same as the Out port.";
                case nameof(MediaCapture.Audio):
                    return "Captured audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new MediaCaptureConfiguration();
    }
}
