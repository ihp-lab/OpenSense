using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public abstract class ExporterConfiguration : ComponentConfiguration {

        protected abstract object CreateInstance(Pipeline pipeline);

        protected abstract void ConnectInput<T>(object instance, InputConfiguration inputConfiguration, IProducer<T> remoteEndProducer);

        public override sealed object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            if (Inputs.Any(i => i.LocalPort?.Index is null)) {
                throw new Exception("exporter stream name not set");
            }
            if (Inputs.Select(i => i.LocalPort.Index).Distinct().Count() != Inputs.Count()) {
                throw new Exception("duplicate exporter stream name");
            }
            var instance = CreateInstance(pipeline);
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
                var connectInputFunc = GetType().GetMethod(nameof(ConnectInput), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dataType);
                connectInputFunc.Invoke(this, new object[] { instance, inputConfig, producer });
            }
            return instance;
        }
    }
}
