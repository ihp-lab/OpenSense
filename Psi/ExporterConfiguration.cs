using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Psi;
using Microsoft.Psi.Data;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public abstract class ExporterConfiguration : ComponentConfiguration {

        private List<Guid> largeMessageInputs = new List<Guid>();

        public List<Guid> LargeMessageInputs {
            get => largeMessageInputs;
            set => SetProperty(ref largeMessageInputs, value);
        }

        protected abstract Exporter CreateExporter(Pipeline pipeline, out object instance);

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            if (Inputs.Any(i => i.LocalPort?.Index is null)) {
                throw new Exception("exporter stream name not set");
            }
            if (Inputs.Select(i => i.LocalPort.Index).Distinct().Count() != Inputs.Count()) {
                throw new Exception("duplicate exporter stream name");
            }
            var exporter = CreateExporter(pipeline, out var instance);
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
                var streamName = (string)inputConfig.LocalPort.Index;
                exporter.Write(producer, streamName, largeMessage, inputConfig.DeliveryPolicy);
            }
            return instance;
        }
    }
}
