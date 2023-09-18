using System;
using Microsoft.Psi;

namespace OpenSense.Components.OpenSmile.Visualizer {
    [Serializable]
    public class OpenSmileVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new OpenSmileVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenSmileVisualizer(pipeline);
    }
}
