using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Emotion.Visualizer {
    [Serializable]
    public class EmotionVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new EmotionVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new EmotionVisualizer(pipeline);
    }
}
