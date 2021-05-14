using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class WaveFileWriterMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that writes an audio stream into a WAVE file.";

        protected override Type ComponentType => typeof(WaveFileWriter);

        public override ComponentConfiguration CreateConfiguration() => new WaveFileWriterConfiguration();
    }
}
