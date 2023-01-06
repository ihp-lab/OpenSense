using System;
using System.Collections.Generic;
using Microsoft.Psi;
using Microsoft.Psi.Data.Json;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Data {
    [Serializable]
    public class JsonStoreImporterConfiguration : ComponentConfiguration {

        private string storeName = string.Empty;

        public string StoreName {
            get => storeName;
            set => SetProperty(ref storeName, value);
        }

        private string rootPath;

        public string RootPath {
            get => rootPath;
            set => SetProperty(ref rootPath, value);
        }

        public override IComponentMetadata GetMetadata() => new JsonStoreImporterMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) => JsonStore.Open(pipeline, StoreName, RootPath);
    }
}
