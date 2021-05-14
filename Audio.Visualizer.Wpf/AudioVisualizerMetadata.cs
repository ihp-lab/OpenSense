using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Audio.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class AudioVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize audio samples.";

        protected override Type ComponentType => typeof(AudioVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new AudioVisualizerConfiguration();
    }
}
