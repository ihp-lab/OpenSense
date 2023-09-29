using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using Microsoft.Psi.Data;

namespace OpenSense.Components.Psi {
    [Serializable]
    public abstract class PsiExporterConfiguration : ExporterConfiguration {

        private List<Guid> largeMessageInputs = new List<Guid>();

        public List<Guid> LargeMessageInputs {
            get => largeMessageInputs;
            set => SetProperty(ref largeMessageInputs, value);
        }

        protected abstract Exporter CreateExporter(Pipeline pipeline);

        protected override sealed object CreateInstance(Pipeline pipeline) {
            var exporter = CreateExporter(pipeline);
            return exporter;
        }

        protected override sealed void ConnectInput<T>(object instance, InputConfiguration inputConfiguration, IProducer<T> remoteEndProducer) {
            var exporter = (Exporter)instance;
            var streamName = (string)inputConfiguration.LocalPort.Index;
            var largeMessage = LargeMessageInputs.Contains(inputConfiguration.Id);
            exporter.Write(remoteEndProducer, streamName, largeMessage, inputConfiguration.DeliveryPolicy);
        }
    }
}
