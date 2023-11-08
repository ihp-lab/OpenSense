using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin.Visualizer {
    [Serializable]
    public class DoubleVisualizerConfiguration : ConventionalComponentConfiguration {

        private static readonly DoubleVisualizerMetadata Metadata = new DoubleVisualizerMetadata();

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DoubleVisualizer(pipeline);
    }
}
