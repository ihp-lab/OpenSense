using System;
using FFMpegInterop;
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

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        private string encoder = "hevc_nvenc";

        public string Encoder {
            get => encoder;
            set => SetProperty(ref encoder, value);
        }

        private PixelFormat targetFormat = PixelFormat.YUV444P;

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

        private string additionalArguments = "-bf 0 -preset p7 -tune lossless";

        public string AdditionalArguments {
            get => additionalArguments;
            set => SetProperty(ref additionalArguments, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileWriter(pipeline) {
            Filename = Filename,
            TimestampFilename = TimestampFilename,
            Encoder = Encoder,
            TargetFormat = TargetFormat,
            TargetWidth = TargetWidth,
            TargetHeight = TargetHeight,
            AdditionalArguments = AdditionalArguments,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
