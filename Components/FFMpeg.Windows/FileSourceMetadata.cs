using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg {
    [Export(typeof(IComponentMetadata))]
    public class FileSourceMetadata : ConventionalComponentMetadata {

        public override string Name => "FFMpeg File Source";

        public override string Description => "[Experimental] FFMpeg File Source.";

        protected override Type ComponentType => typeof(FileSource);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FileSource.Out):
                    return "The output.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FileSourceConfiguration();
    }
}

