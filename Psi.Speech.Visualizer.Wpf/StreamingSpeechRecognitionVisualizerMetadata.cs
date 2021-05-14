using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Speech.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class StreamingSpeechRecognitionVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize speech recognition output.";

        protected override Type ComponentType => typeof(StreamingSpeechRecognitionVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new StreamingSpeechRecognitionVisualizerConfiguration();
    }
}
