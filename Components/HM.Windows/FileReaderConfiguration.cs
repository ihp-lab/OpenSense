using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class FileReaderConfiguration : ConventionalComponentConfiguration {

        #region File Settings
        private string filename = string.Empty;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }
        #endregion

        #region Timestamp Settings
        private StartTimeMode startTimeMode = StartTimeMode.PipelineStartTime;

        public StartTimeMode StartTimeMode {
            get => startTimeMode;
            set => SetProperty(ref startTimeMode, value);
        }

        private DateTime manualStartTime = DateTime.MinValue;

        public DateTime ManualStartTime {
            get => manualStartTime;
            set => SetProperty(ref manualStartTime, value);
        }
        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new FileReaderMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileReader(pipeline, Filename) {
            StartTimeMode = StartTimeMode,
            ManualStartTime = ManualStartTime,
            ProcessRemainingBeforeStop = ProcessRemainingBeforeStop,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
