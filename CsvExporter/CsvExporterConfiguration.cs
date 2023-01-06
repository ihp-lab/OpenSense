using System;
using System.IO;
using Microsoft.Psi;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi;

namespace OpenSense.Components.CsvExporter {
    [Serializable]
    public class CsvExporterConfiguration : ExporterConfiguration {

        private string filename = Path.GetTempFileName();

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private int maxRecursionDepth = int.MaxValue;

        public int MaxRecursionDepth {
            get => maxRecursionDepth;
            set => SetProperty(ref maxRecursionDepth, value);
        }

        private string nullValueResultString = "null";

        public string NullValueResultString {
            get => nullValueResultString;
            set => SetProperty(ref nullValueResultString, value);
        }

        public override IComponentMetadata GetMetadata() => new CsvExporterMetadata();

        protected override object CreateInstance(Pipeline pipeline) {
            var result = new CsvExporter(pipeline, Filename) { 
                MaxRecursionDepth = MaxRecursionDepth,
                NullValueResultString = NullValueResultString,
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
