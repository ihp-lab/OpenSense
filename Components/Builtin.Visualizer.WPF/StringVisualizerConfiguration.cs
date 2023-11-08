#nullable enable

using System;
using Microsoft.Psi;
using OpenSense.Components;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Serializable]
    public class StringVisualizerConfiguration : ConventionalComponentConfiguration {

        private static readonly StringVisualizerMetadata Metadata = new StringVisualizerMetadata();

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new StringVisualizer(pipeline);
    }
}
