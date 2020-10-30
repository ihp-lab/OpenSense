using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class MediaCaptureMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that captures and streams video and audio from a camera.";

        protected override Type ComponentType => typeof(MediaCapture);

        public override ComponentConfiguration CreateConfiguration() => new MediaCaptureConfiguration();
    }
}
