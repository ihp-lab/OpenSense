using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Builtin.Visualizer {
    [Serializable]
    public class BooleanVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new BooleanVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BooleanVisualizer(pipeline);
    }
}
