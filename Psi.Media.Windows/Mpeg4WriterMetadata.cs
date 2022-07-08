using System;
using System.Composition;
using Microsoft.Psi.Media;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Media {
    [Export(typeof(IComponentMetadata))]
    public class Mpeg4WriterMetadata : ConventionalComponentMetadata {

        public override string Description => "Writes video and audio into an MPEG-4 file.";

        protected override Type ComponentType => typeof(Mpeg4Writer);

        public override string Name => "MPEG-4 Writer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(Mpeg4Writer.In):
                    return "[Required] Images. Same as the port ImageIn.";
                case nameof(Mpeg4Writer.ImageIn):
                    return "[Required] Images. Same as the port In.";
                case nameof(Mpeg4Writer.AudioIn):
                    return "[Optional] Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new Mpeg4WriterConfiguration();
    }
}
