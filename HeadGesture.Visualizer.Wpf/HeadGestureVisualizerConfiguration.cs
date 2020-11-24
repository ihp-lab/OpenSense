using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.HeadGesture.Visualizer {
    [Serializable]
    public class HeadGestureVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new HeadGestureVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new HeadGestureVisualizer(pipeline);
    }
}
