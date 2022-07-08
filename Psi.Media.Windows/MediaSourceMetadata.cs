using System;
using System.Composition;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class MediaSourceMetadata : ConventionalComponentMetadata {

        public override string Description => "Reads video and audio from a media file.";

        protected override Type ComponentType => typeof(MediaSource);

        public override string Name => "Media Source";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(MediaSource.Image):
                    return "Images.";
                case nameof(MediaSource.Audio):
                    return "Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new MediaSourceConfiguration();
    }
}
