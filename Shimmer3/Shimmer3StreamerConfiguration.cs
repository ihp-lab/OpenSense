using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Shimmer3 {
    [Serializable]
    public class Shimmer3StreamerConfiguration : ConventionalComponentConfiguration {

        private DeviceConfiguration raw = new DeviceConfiguration();

        public DeviceConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        private double bufferTimeInMillisecond = 100;

        public double BufferTimeInMillisecond {
            get => bufferTimeInMillisecond;
            set => SetProperty(ref bufferTimeInMillisecond, value);
        }

        public override IComponentMetadata GetMetadata() => new Shimmer3StreamerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new Shimmer3Streamer(pipeline, Raw) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerProvider)) as ILoggerProvider)?.CreateLogger(Name),
            BufferTimeSpan = TimeSpan.FromMilliseconds(BufferTimeInMillisecond),
        };
    }
}
