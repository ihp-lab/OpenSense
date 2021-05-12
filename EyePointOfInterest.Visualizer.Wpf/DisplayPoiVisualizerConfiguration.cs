using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.EyePointOfInterest.Visualizer {
    [Serializable]
    public class DisplayPoiVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new DisplayPoiVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new DisplayPoiVisualizer(pipeline);
    }
}
