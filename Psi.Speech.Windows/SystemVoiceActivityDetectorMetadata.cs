using System;
using System.Composition;
using Microsoft.Psi.Speech;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Speech {
    [Export(typeof(IComponentMetadata))]
    public class SystemVoiceActivityDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that performs voice activity detection by using the desktop speech recognition engine from `System.Speech`.";

        protected override Type ComponentType => typeof(SystemVoiceActivityDetector);

        public override ComponentConfiguration CreateConfiguration() => new SystemVoiceActivityDetectorConfiguration();
    }
}
