using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Remoting;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Serializable]
    public class RemoteImporterConfiguration : ComponentConfiguration {

        private TimeInterval Replay => ReplayDescriptor.ReplayAll.Interval;

        private string host = "localhost";

        public string Host {
            get => host;
            set => SetProperty(ref host, value);
        }

        private int port = 11411;

        public int Port {
            get => port;
            set => SetProperty(ref port, value);
        }

        private bool allowSequenceRestart = true;

        public bool AllowSequenceRestart {
            get => allowSequenceRestart;
            set => SetProperty(ref allowSequenceRestart, value);
        }

        private int connectionTimeoutSeconds = 10;

        public int ConnectionTimeoutSeconds {
            get => connectionTimeoutSeconds;
            set => SetProperty(ref connectionTimeoutSeconds, value);
        }

        public override IComponentMetadata GetMetadata() => new RemoteImporterMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            var remoteImporter = new RemoteImporter(pipeline, Replay, Host, Port, AllowSequenceRestart);
            var connected = remoteImporter.Connected.WaitOne(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));//otherwise the Importer field will be empty
            if (!connected) {
                throw new TimeoutException($"connection with {Host}:{Port} timed out");
            }
            return remoteImporter;
        }
    }
}
