using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Imaging.Visualizer {
    [Serializable]
    public class DepthImageVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new DepthImageVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DepthImageVisualizer(pipeline);
    }
}
