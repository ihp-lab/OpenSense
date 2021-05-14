using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Emotion.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class EmotionVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize emotion values.";

        protected override Type ComponentType => typeof(EmotionVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new EmotionVisualizerConfiguration();
    }
}
