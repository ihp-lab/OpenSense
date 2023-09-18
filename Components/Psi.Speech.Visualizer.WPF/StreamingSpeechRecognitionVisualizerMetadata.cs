using System;
using System.Composition;

namespace OpenSense.Components.Psi.Speech.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class StreamingSpeechRecognitionVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes speech recognition results.";

        protected override Type ComponentType => typeof(StreamingSpeechRecognitionVisualizer);

        public override string Name => "Speech Recognition Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(StreamingSpeechRecognitionVisualizer.In):
                    return "[Required] Speech recognition results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new StreamingSpeechRecognitionVisualizerConfiguration();
    }
}
