﻿using System;
using Microsoft.Psi;

namespace OpenSense.Components.Audio.Visualizer {
    [Serializable]
    public class AudioVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new AudioVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AudioVisualizer(pipeline);
    }
}
