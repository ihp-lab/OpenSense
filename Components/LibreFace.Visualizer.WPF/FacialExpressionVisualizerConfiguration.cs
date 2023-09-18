using System;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace.Visualizer {
    [Serializable]
    public class FacialExpressionVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new FacialExpressionVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FacialExpressionVisualizer(pipeline);
    }
}
