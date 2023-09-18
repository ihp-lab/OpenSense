using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using Microsoft.Psi.Data;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public class PsiStoreImporterMetadata : IComponentMetadata {

        public string Name => "\\psi Store Importer";

        public string Description => @"Reads streams from a \psi store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ImporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new PsiStoreImporterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            var importer = (PsiImporter)instance;
            var streamName = (string)portConfiguration.Index;
            return importer.OpenStream<T>(streamName);
        }
    }
}
