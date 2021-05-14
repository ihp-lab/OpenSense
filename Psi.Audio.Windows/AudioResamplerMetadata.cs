using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioResamplerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that resamples an audio stream into a different format.";

        protected override Type ComponentType => typeof(AudioResampler);

        public override ComponentConfiguration CreateConfiguration() => new AudioResamplerConfiguration();
    }
}
