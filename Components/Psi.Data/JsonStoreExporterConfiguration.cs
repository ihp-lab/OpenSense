using System;
using Microsoft.Psi;
using Microsoft.Psi.Data.Json;

namespace OpenSense.Components.Psi.Data {
    [Serializable]
    public class JsonStoreExporterConfiguration : ExporterConfiguration {//In /psi JsonExporter is not a subclass of Exporter, so we cannot inherit from PsiExporterConfiguration.

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

        private bool createSubdirectory;

        public bool CreateSubdirectory {
            get => createSubdirectory;
            set => SetProperty(ref createSubdirectory, value);
        }

        public override IComponentMetadata GetMetadata() => new JsonStoreExporterMetadata();

        protected override sealed object CreateInstance(Pipeline pipeline) {
            var exporter = JsonStore.Create(pipeline, StoreName, RootPath, CreateSubdirectory);
            return exporter;
        }

        protected override sealed void ConnectInput<T>(object instance, InputConfiguration inputConfiguration, IProducer<T> remoteEndProducer) {
            var exporter = (JsonExporter)instance;
            var streamName = (string)inputConfiguration.LocalPort.Index;
            exporter.Write((Emitter<T>)remoteEndProducer, streamName,inputConfiguration.DeliveryPolicy);
        }
    }
}
