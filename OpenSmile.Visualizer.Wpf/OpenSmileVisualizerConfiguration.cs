using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenSmile.Visualizer {
    [Serializable]
    public class OpenSmileVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new OpenSmileVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenSmileVisualizer(pipeline);
    }
}
