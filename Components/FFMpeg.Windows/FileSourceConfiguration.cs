using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    [Serializable]
    public class FileSourceConfiguration : ConventionalComponentConfiguration {

        private static readonly FileSourceMetadata Metadata = new FileSourceMetadata();

        #region Options
        private string filename = string.Empty;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private PixelFormat targetFormat = PixelFormat.RGB_24bpp;

        public PixelFormat TargetFormat {
            get => targetFormat;
            set => SetProperty(ref targetFormat, value);
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

        private bool onlyKeyFrames;

        public bool OnlyKeyFrames {
            get => onlyKeyFrames;
            set => SetProperty(ref onlyKeyFrames, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileSource(pipeline, Filename) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
            TargetFormat = TargetFormat,
            OnlyKeyFrames = OnlyKeyFrames,
            TargetWidth = TargetWidth,
            TargetHeight = TargetHeight,
        };
    }
}
