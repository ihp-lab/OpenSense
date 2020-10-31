using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public class LocalExporterConfiguration : ComponentConfiguration {

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

        private ObservableCollection<Guid> largeMessageInputs = new ObservableCollection<Guid>();
        
        public ObservableCollection<Guid> LargeMessageInputs {
            get => largeMessageInputs;
            set => SetProperty(ref largeMessageInputs, value);
        }

        public override IComponentMetadata GetMetadata() => new LocalExporterMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            if (Inputs.Any(i => i.LocalPort?.Index is null)) {
                throw new Exception("exporter stream name not set");
            }
            if (Inputs.Select(i => i.LocalPort.Index).Distinct().Count() != Inputs.Count()) {
                throw new Exception("duplicate exporter stream name");
            }
            var exporter = PsiStore.Create(pipeline, StoreName, RootPath, CreateSubdirectory);
            var configurations = instantiatedComponents.Select(i => i.Configuration).ToArray();
            foreach (var inputConfig in Inputs) {
                var remoteEnv = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var remotePortMeta = remoteEnv.FindPortMetadata(inputConfig.RemotePort);
                var dataType = remoteEnv.Configuration.FindOutputPortDataType(remotePortMeta, configurations);
                if (dataType is null) {
                    throw new Exception("unknown port transmission data type");
                }
                var getProducerFunc = typeof(HelperExtensions).GetMethod(nameof(HelperExtensions.GetProducer)).MakeGenericMethod(dataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnv, inputConfig.RemotePort });
                var largeMessage = LargeMessageInputs.Contains(inputConfig.Id);
                var streamName = (string)inputConfig.LocalPort.Identifier;
                exporter.Write(producer, streamName, largeMessage, inputConfig.DeliveryPolicy);
            }
            return exporter;
        }
    }
}
