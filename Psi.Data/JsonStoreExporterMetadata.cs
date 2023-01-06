using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Data {
    [Export(typeof(IComponentMetadata))]
    public class JsonStoreExporterMetadata : IComponentMetadata {

        public string Name => "JSON Store Exporter";

        public string Description => @"Write streams to a JSON store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new JsonStoreExporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
