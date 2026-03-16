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
        private bool abortOnStop;

        public bool AbortOnStop {
            get => abortOnStop;
            set => SetProperty(ref abortOnStop, value);
        }
        #endregion

        #region Input Validation
        private int? inputBitDepth;

        /// <summary>
        /// Expected input bit depth. Null = auto (detect from input at runtime). Set to validate input.
        /// </summary>
        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }

        private ChromaFormat? inputChromaFormat;

        /// <summary>
        /// Expected input chroma format. Null = auto (detect from input at runtime). Set to validate input.
        /// </summary>
        public ChromaFormat? InputChromaFormat {
            get => inputChromaFormat;
            set => SetProperty(ref inputChromaFormat, value);
        }
        #endregion

        #region Encoding
        private int? internalBitDepth;

        /// <summary>
        /// Internal bit depth for encoding. Null = same as input bit depth.
        /// </summary>
        public int? InternalBitDepth {
            get => internalBitDepth;
            set => SetProperty(ref internalBitDepth, value);
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
            AbortOnStop = AbortOnStop,
            InputBitDepth = InputBitDepth,
            InputChromaFormat = InputChromaFormat,
            InternalBitDepth = InternalBitDepth,
            EncoderConfiguration = Raw.Clone(),
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
