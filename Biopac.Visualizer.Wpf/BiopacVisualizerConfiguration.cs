using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Biopac.Visualizer {
    [Serializable]
    public class BiopacVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new BiopacVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new BiopacVisualizer(pipeline);
    }
}
