using System;
using System.Composition;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class MediaSourceMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that streams video and audio from a media file.";

        protected override Type ComponentType => typeof(MediaSource);

        public override ComponentConfiguration CreateConfiguration() => new MediaSourceConfiguration();
    }
}
