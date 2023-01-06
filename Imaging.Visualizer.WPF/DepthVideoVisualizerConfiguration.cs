using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Serializable]
    public class DepthVideoVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new DepthVideoVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DepthVideoVisualizer(pipeline);
    }
}
