using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.HeadGesture.Visualizer {
    [Serializable]
    public class HeadGestureVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new HeadGestureVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HeadGestureVisualizer(pipeline);
    }
}
