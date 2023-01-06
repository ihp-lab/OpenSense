using System;
using System.Collections.Generic;
using System.Composition;
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

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
