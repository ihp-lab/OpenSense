using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Imaging.Visualizer {
    [Serializable]
    public class ImageVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ImageVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageVisualizer(pipeline);
    }
}
