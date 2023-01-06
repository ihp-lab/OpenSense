using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Serializable]
    public class ColorVideoVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ColorVideoVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ColorVideoVisualizer(pipeline);
    }
}
