using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Export(typeof(IComponentMetadata))]
    public class PsiStoreExporterMetadata : IComponentMetadata {

        public string Name => "Psi Store Exporter";

        public string Description => @"Write streams to a \psi store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] { 
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new PsiStoreExporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
