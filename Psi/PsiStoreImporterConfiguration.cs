using System;
using System.Collections.Generic;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public class PsiStoreImporterConfiguration : ComponentConfiguration {

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

        public override IComponentMetadata GetMetadata() => new PsiStoreImporterMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) => PsiStore.Open(pipeline, StoreName, RootPath);
    }
}
