using System.Collections.Generic;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging {
    public class FlipColorVideoMetadata : IComponentMetadata {

        public string Name => typeof(FlipColorVideo).FullName;

        public string Description => "";

        public IReadOnlyList<IPortMetadata> Ports => new[] { 
            new StaticPortMetadata(typeof(FlipColorVideo).GetProperty(nameof(FlipColorVideo.In))),
            new StaticPortMetadata(typeof(FlipColorVideo).GetProperty(nameof(FlipColorVideo.Out))),
        };

        public ComponentConfiguration CreateConfiguration() => new FlipColorVideoConfiguration();

        public IProducer<T> GetOutputProducer<T>(object instance, PortConfiguration portConfiguration) => this.GetOutputProducerOfStaticPort<T>(instance, portConfiguration);
    }
}
