using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class Mp4MuxerConfiguration : ConventionalComponentConfiguration {

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

        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new Mp4MuxerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new Mp4Muxer(pipeline) {
            Filename = Filename,
            TimestampFilename = TimestampFilename,
            ProcessRemainingBeforeStop = ProcessRemainingBeforeStop,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
