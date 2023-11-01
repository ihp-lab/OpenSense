using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.FFMpeg {
    [Serializable]
    public class FileSourceConfiguration : ConventionalComponentConfiguration {

        private static readonly FileSourceMetadata Metadata = new FileSourceMetadata();

        #region Settings
        private string filename = string.Empty;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        } 
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileSource(pipeline, Filename) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
