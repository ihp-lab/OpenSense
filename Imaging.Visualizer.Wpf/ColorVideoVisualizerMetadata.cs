using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class ColorVideoVisualizerMetadata : IComponentMetadata {

        public string Name => typeof(ColorVideoVisualizer).FullName;

        public string Description => "Visualize color images.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new StaticPortMetadata(typeof(ColorVideoVisualizer).GetProperty(nameof(ColorVideoVisualizer.In))),
        };

        public ComponentConfiguration CreateConfiguration() => new ColorVideoVisualizerConfiguration();

        public IProducer<T> GetOutputProducer<T>(object instance, PortConfiguration portConfiguration) => this.GetOutputProducerOfStaticPort<T>(instance, portConfiguration);
    }
}
