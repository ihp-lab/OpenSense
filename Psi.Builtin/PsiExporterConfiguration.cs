using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Data;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Serializable]
    public abstract class PsiExporterConfiguration : ExporterConfiguration {

        private Exporter exporter;

        private List<Guid> largeMessageInputs = new List<Guid>();

        public List<Guid> LargeMessageInputs {
            get => largeMessageInputs;
            set => SetProperty(ref largeMessageInputs, value);
        }

        protected abstract Exporter CreateExporter(Pipeline pipeline, out object instance);

        protected override sealed object CreateInstance(Pipeline pipeline) {
            exporter = CreateExporter(pipeline, out var instance);
            return instance;
        }

        protected override sealed void ConnectInput<T>(object instance, InputConfiguration inputConfiguration, IProducer<T> remoteEndProducer) {
            var streamName = (string)inputConfiguration.LocalPort.Index;
            var largeMessage = LargeMessageInputs.Contains(inputConfiguration.Id);
            exporter.Write((dynamic)remoteEndProducer, streamName, largeMessage, inputConfiguration.DeliveryPolicy);
        }
    }
}
