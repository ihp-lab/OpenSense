using System;
using Microsoft.Psi;

namespace OpenSense.Components.Psi.Imaging.Visualizer {
    [Serializable]
    public class ImageVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ImageVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageVisualizer(pipeline);
    }
}
