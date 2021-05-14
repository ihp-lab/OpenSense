using System;
using System.Composition;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class Mpeg4WriterMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that writes video and audio streams into an MPEG-4 file.";

        protected override Type ComponentType => typeof(Mpeg4Writer);

        public override ComponentConfiguration CreateConfiguration() => new Mpeg4WriterConfiguration();
    }
}
