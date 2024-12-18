﻿using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using Microsoft.Psi.Data.Json;

namespace OpenSense.Components.Psi.Data {
    [Export(typeof(IComponentMetadata))]
    public class JsonStoreImporterMetadata : IComponentMetadata {

        public string Name => "JSON Store Importer";

        public string Description => @"Read streams from a JSON store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ImporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new JsonStoreImporterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            var importer = (JsonGenerator)instance;
            var streamName = (string)portConfiguration.Index;
            return importer.OpenStream<T>(streamName);
        }
    }
}
