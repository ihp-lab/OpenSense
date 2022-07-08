using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Audio.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class AudioVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes audio signal.";

        protected override Type ComponentType => typeof(AudioVisualizer);

        public override string Name => "Audio Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(AudioVisualizer.In):
                    return "[Required] Audio signal.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AudioVisualizerConfiguration();
    }
}
