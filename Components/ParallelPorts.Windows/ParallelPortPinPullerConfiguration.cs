using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.ParallelPorts {
    [Serializable]
    public class ParallelPortPinPullerConfiguration : ConventionalComponentConfiguration {

        private static readonly ParallelPortPinPullerMetadata Metadata = new();

        #region Settings
        private short memoryAddress = 0x378;

        /// <remarks>
        /// Inpoutx64 uses signed short for memory address. Here we follow its convention.
        /// </remarks>
        public short MemoryAddress {
            get => memoryAddress;
            set => SetProperty(ref memoryAddress, value);
        }

        private bool setOnStart = false;

        public bool SetOnStart {
            get => setOnStart;
            set => SetProperty(ref setOnStart, value);
        }

        private byte setOnStartValue = 0;

        public byte SetOnStartValue {
            get => setOnStartValue;
            set => SetProperty(ref setOnStartValue, value);
        }

        private bool setAfterStop = false;

        public bool SetAfterStop {
            get => setAfterStop;
            set => SetProperty(ref setAfterStop, value);
        }

        private byte setAfterStopValue = 0;

        public byte SetAfterStopValue {
            get => setAfterStopValue;
            set => SetProperty(ref setAfterStopValue, value);
        }

        private bool useSourceOriginatingTime = false;

        public bool UseSourceOriginatingTime {
            get => useSourceOriginatingTime;
            set => SetProperty(ref useSourceOriginatingTime, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ParallelPortPinPuller(pipeline) {
            MemoryAddress = MemoryAddress,
            SetOnStart = SetOnStart,
            SetOnStartValue = SetOnStartValue,
            SetAfterStop = SetAfterStop,
            SetAfterStopValue = SetAfterStopValue,
            UseSourceOriginatingTime = UseSourceOriginatingTime,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
