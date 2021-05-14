using System;
using System.Composition;
using Microsoft.Psi.Audio;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Audio {
    [Export(typeof(IComponentMetadata))]
    public class AudioCaptureMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that captures and streams audio from an input device such as a microphone.";

        protected override Type ComponentType => typeof(AudioCapture);

        public override ComponentConfiguration CreateConfiguration() => new AudioCaptureConfiguration();
    }
}
