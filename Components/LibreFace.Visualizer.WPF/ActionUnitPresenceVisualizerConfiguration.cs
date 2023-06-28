using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Serializable]
    public class ActionUnitPresenceVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ActionUnitPresenceVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ActionUnitPresenceVisualizer(pipeline);
    }
}
