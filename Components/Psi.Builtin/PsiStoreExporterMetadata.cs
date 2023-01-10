﻿using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public class PsiStoreExporterMetadata : IComponentMetadata {

        public string Name => "\\psi Store Exporter";

        public string Description => @"Writes streams to a \psi store.";

        public IReadOnlyList<IPortMetadata> Ports => new[] { 
            new ExporterPortMetadata(),
        };

        public ComponentConfiguration CreateConfiguration() => new PsiStoreExporterConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) => throw new InvalidOperationException();
    }
}