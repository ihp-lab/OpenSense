using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Emotion.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class EmotionVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualizes emotion values. Requires emotion detector outputs.";

        protected override Type ComponentType => typeof(EmotionVisualizer);

        public override string Name => "Emotion Visualizer";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(EmotionVisualizer.In):
                    return "[Required] Emotion values.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new EmotionVisualizerConfiguration();
    }
}
