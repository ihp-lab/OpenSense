using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Audio.Visualizer {
    [Serializable]
    public class AudioVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new AudioVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioVisualizer(pipeline);
    }
}
