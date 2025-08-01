using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.FFMpeg {
    [Serializable]
    public class FileWriterConfiguration : ConventionalComponentConfiguration {

        private static readonly FileWriterMetadata Metadata = new FileWriterMetadata();

        #region Options
        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private int targetWidth = 0;

        public int TargetWidth {
            get => targetWidth;
            set => SetProperty(ref targetWidth, value);
        }

        private int targetHeight = 0;

        public int TargetHeight {
            get => targetHeight;
            set => SetProperty(ref targetHeight, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileWriter(pipeline, Filename, TargetWidth, TargetHeight) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
