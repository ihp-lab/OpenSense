using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Export(typeof(IComponentMetadata))]
    public class LocalExporterMetadata : IComponentMetadata {

        public string Name => "Local Exporter";

        public string Description => "Write streams to local store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] { 
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new LocalExporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
