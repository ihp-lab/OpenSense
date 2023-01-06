using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioResamplerMetadata : ConventionalComponentMetadata {

        public override string Description => "Converts and resamples audio signal to another format.";

        protected override Type ComponentType => typeof(AudioResampler);

        public override string Name => "Audio Resampler";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AudioResampler.In):
                    return "[Required] Audio signal to be processed.";
                case nameof(AudioResampler.Out):
                    return "Processed audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AudioResamplerConfiguration();
    }
}
