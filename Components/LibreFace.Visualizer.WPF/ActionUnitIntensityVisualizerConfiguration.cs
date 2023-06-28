using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Serializable]
    public class ActionUnitIntensityVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ActionUnitIntensityVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ActionUnitIntensityVisualizer(pipeline);
    }
}
