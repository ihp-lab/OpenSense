using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.SerialPorts {
    [Serializable]
    public class SerialPortPinPullerConfiguration : ConventionalComponentConfiguration {

        private static readonly SerialPortPinPullerMetadata Metadata = new ();

        #region Settings
        private string portName = string.Empty;

        public string PortName {
            get => portName;
            set => SetProperty(ref portName, value);
        }

        private bool pullUpDtrOnStart = false;

        public bool PullUpDtrOnStart {
            get => pullUpDtrOnStart;
            set => SetProperty(ref pullUpDtrOnStart, value);
        }

        private bool dtrUseSourceOriginatingTime = false;

        public bool DtrUseSourceOriginatingTime {
            get => dtrUseSourceOriginatingTime;
            set => SetProperty(ref dtrUseSourceOriginatingTime, value);
        }

        private bool pullUpRtsOnStart = false;

        public bool PullUpRtsOnStart {
            get => pullUpRtsOnStart;
            set => SetProperty(ref pullUpRtsOnStart, value);
        }

        private bool rtsUseSourceOriginatingTime = false;

        public bool RtsUseSourceOriginatingTime {
            get => rtsUseSourceOriginatingTime;
            set => SetProperty(ref rtsUseSourceOriginatingTime, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new SerialPortPinPuller(pipeline, PortName) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
            PullUpDtrOnStart = PullUpDtrOnStart,
            DtrUseSourceOriginatingTime = DtrUseSourceOriginatingTime,
            PullUpRtsOnStart = PullUpRtsOnStart,
            RtsUseSourceOriginatingTime = RtsUseSourceOriginatingTime,
        };
    }
}
