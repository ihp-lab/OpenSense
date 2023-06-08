using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Serializable]
    public class ActionUnitVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ActionUnitVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ActionUnitVisualizer(pipeline);
    }
}
