using System;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class FileWriterConfiguration : ConventionalComponentConfiguration {

        #region Options
        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        private EncoderConfig raw = new EncoderConfig();

        public EncoderConfig Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new FileWriterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileWriter(pipeline) {
            Filename = Filename,
            TimestampFilename = TimestampFilename,
            EncoderConfiguration = Raw.Clone(),
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
