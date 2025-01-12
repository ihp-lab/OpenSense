﻿using System;
using System.Collections.Generic;
using System.Composition;
using Microsoft.Psi;
using OpenSense.Components.Psi;

namespace OpenSense.Components.CsvExporter {
    [Export(typeof(IComponentMetadata))]
    public class CsvExporterMetadata : IComponentMetadata {

        public string Name => "CSV File Exporter";

        public string Description => @"Write streams to a CSV file.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new CsvExporterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}
