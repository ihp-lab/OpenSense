using System;
using Microsoft.Psi;

namespace OpenSense.Components.Biopac.Visualizer {
    [Serializable]
    public class BiopacVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new BiopacVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new BiopacVisualizer(pipeline);
    }
}
