using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin.Visualizer {
    [Serializable]
    public class DoubleVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new DoubleVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DoubleVisualizer(pipeline);
    }
}
