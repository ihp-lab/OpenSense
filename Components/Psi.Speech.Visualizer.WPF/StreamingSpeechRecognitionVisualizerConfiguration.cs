using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Speech.Visualizer {
    [Serializable]
    public class StreamingSpeechRecognitionVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new StreamingSpeechRecognitionVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new StreamingSpeechRecognitionVisualizer(pipeline);
    }
}
