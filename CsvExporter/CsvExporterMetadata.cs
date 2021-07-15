using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi;

namespace OpenSense.Component.CsvExporter {
    [Export(typeof(IComponentMetadata))]
    public class CsvExporterMetadata : IComponentMetadata {

        public string Name => "CSV File Exporter";

        public string Description => @"Write streams to a CSV file.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new CsvExporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
