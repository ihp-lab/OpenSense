using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin.Visualizer {
    [Serializable]
    public class BooleanVisualizerConfiguration : ConventionalComponentConfiguration {

        private readonly static BooleanVisualizerMetadata Metadata = new BooleanVisualizerMetadata();

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BooleanVisualizer(pipeline);
    }
}
