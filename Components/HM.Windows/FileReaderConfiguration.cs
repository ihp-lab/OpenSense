using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class FileReaderConfiguration : ConventionalComponentConfiguration {

        private static readonly FileReaderMetadata Metadata = new FileReaderMetadata();

        #region Options
        private string filename = string.Empty;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool parseFilenameTimestamp;

        public bool ParseFilenameTimestamp {
            get => parseFilenameTimestamp;
            set => SetProperty(ref parseFilenameTimestamp, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileReader(pipeline, Filename) {
            ParseFilenameTimestamp = ParseFilenameTimestamp,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
