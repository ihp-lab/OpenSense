using System;
using System.IO;
using Microsoft.Psi;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi;

namespace OpenSense.Component.CsvExporter {
    [Serializable]
    public class CsvExporterConfiguration : ExporterConfiguration {

        private string filename = Path.GetTempFileName();

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        public override IComponentMetadata GetMetadata() => new CsvExporterMetadata();

        protected override object CreateInstance(Pipeline pipeline) {
            var result = new CsvExporter(pipeline, Filename) { 
            };
            return result;
        }

        protected override void ConnectInput<T>(object instance, InputConfiguration inputConfiguration, IProducer<T> remoteEndProducer) {
            var streamName = (string)inputConfiguration.LocalPort.Index;
            var exporter = (CsvExporter)instance;
            exporter.WriteStream((dynamic)remoteEndProducer, streamName, inputConfiguration.DeliveryPolicy);
        }

        protected override void FinalizeInstantiation(object instance) {
            var exporter = (CsvExporter)instance;
            exporter.FinishInstantiation();
        }
    }
}
