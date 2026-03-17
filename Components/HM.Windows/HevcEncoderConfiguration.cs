using System;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class HevcEncoderConfiguration : ConventionalComponentConfiguration {

        #region Options
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

        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        } 
        #endregion

        public override IComponentMetadata GetMetadata() => new HevcEncoderMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HevcEncoder(pipeline) {
            InputBitDepth = InputBitDepth,
            InputChromaFormat = InputChromaFormat,
            InternalBitDepth = InternalBitDepth,
            EncoderConfiguration = Raw.Clone(),
            ProcessRemainingBeforeStop = ProcessRemainingBeforeStop,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
