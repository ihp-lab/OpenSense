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

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
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

        private int gopSize = 30;//TODO: Error closing the codec if set to 0, don't know why.

        public int GopSize {
            get => gopSize;
            set => SetProperty(ref gopSize, value);
        }

        private int maxBFrames = 0;//TODO: Error writing packet when set to non-zero: av_interleaved_write_frame() fails to write the output packet, possibly due to timestamp issues, as there is no error when packets are manually ordered. However, the video cannot be played back without artifacts if manually ordered. Need to learn how B-frames are saved.

        public int MaxBFrames {
            get => maxBFrames;
            set => SetProperty(ref maxBFrames, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileWriter(pipeline) {
            Filename = Filename,
            TimestampFilename = TimestampFilename,
            TargetWidth = TargetWidth,
            TargetHeight = TargetHeight,
            GopSize = GopSize,
            MaxBFrames = MaxBFrames,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
