using System;
using Microsoft.Psi;
using Microsoft.Psi.Data;
using Microsoft.Psi.Remoting;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Serializable]
    public class RemoteExporterConfiguration : PsiExporterConfiguration {

        private int port = 11411;

        public int Port {
            get => port;
            set => SetProperty(ref port, value);
        }

        private TransportKind transport = TransportKind.NamedPipes;

        public TransportKind Transport {
            get => transport;
            set => SetProperty(ref transport, value);
        }

        private long maxBytesPerSecond = long.MaxValue;

        public long MaxBytesPerSecond {
            get => maxBytesPerSecond;
            set => SetProperty(ref maxBytesPerSecond, value);
        }

        private double bytesPerSecondSmoothingWindowSeconds = 5;

        public double BytesPerSecondSmoothingWindowSeconds {
            get => bytesPerSecondSmoothingWindowSeconds;
            set => SetProperty(ref bytesPerSecondSmoothingWindowSeconds, value);
        }

        public override IComponentMetadata GetMetadata() => new RemoteExporterMetadata();

        protected override Exporter CreateExporter(Pipeline pipeline, out object instance) {
            var remoteExporter = new RemoteExporter(pipeline, Port, Transport, MaxBytesPerSecond, BytesPerSecondSmoothingWindowSeconds);
            instance = remoteExporter;
            return remoteExporter.Exporter;
        }
            
    }
}
