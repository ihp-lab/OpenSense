using System;
using Microsoft.Psi;

namespace OpenSense.Components.Emotion.Visualizer {
    [Serializable]
    public class EmotionVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new EmotionVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new EmotionVisualizer(pipeline);
    }
}
