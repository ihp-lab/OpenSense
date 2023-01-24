using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public class RemoteExporterMetadata : IComponentMetadata {

        public string Name => "Remote Exporter";

        public string Description => "Broadcasts streams.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new RemoteExporterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
