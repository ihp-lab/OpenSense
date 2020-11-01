using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using Microsoft.Psi.Remoting;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Export(typeof(IComponentMetadata))]
    public class RemoteImporterMetadata : IComponentMetadata {

        public string Name => "Remote Importer";

        public string Description => "Read streams from a remote host.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ImporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new RemoteImporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) {
            var importer = (RemoteImporter)instance;
            var streamName = (string)portConfiguration.Index;
            return importer.Importer.OpenStream<T>(streamName);
        }
    }
}
