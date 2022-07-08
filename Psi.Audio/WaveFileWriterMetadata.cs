using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class WaveFileWriterMetadata : ConventionalComponentMetadata {

        public override string Description => "Writes audio signal to a WAVE file.";

        protected override Type ComponentType => typeof(WaveFileWriter);

        public override string Name => "Wave File Writer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(WaveFileWriter.In):
                    return "[Required] Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new WaveFileWriterConfiguration();
    }
}
