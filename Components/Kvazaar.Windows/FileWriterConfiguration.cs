using System;
using KvazaarInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.Kvazaar {
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

#if FIXED_BIT_DEPTH
        public int? InputBitDepth => Picture.MaxBitDepth;
#else
        private int? inputBitDepth;

        public int? InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }
#endif

        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        }

        #region Encoder Configuration

        #region Core Quality
        private bool lossless;

        public bool Lossless {
            get => lossless;
            set => SetProperty(ref lossless, value);
        }

        private int qp = 22;

        public int QP {
            get => qp;
            set => SetProperty(ref qp, value);
        }
        #endregion

        #region GOP Structure
        private int intraPeriod = 64;

        public int IntraPeriod {
            get => intraPeriod;
            set => SetProperty(ref intraPeriod, value);
        }

        private int vpsPeriod;

        public int VpsPeriod {
            get => vpsPeriod;
            set => SetProperty(ref vpsPeriod, value);
        }

        private int gopLen = 4;

        public int GopLen {
            get => gopLen;
            set => SetProperty(ref gopLen, value);
        }

        private bool gopLowDelay = true;

        public bool GopLowDelay {
            get => gopLowDelay;
            set => SetProperty(ref gopLowDelay, value);
        }

        private bool openGop = true;

        public bool OpenGop {
            get => openGop;
            set => SetProperty(ref openGop, value);
        }

        private int refFrames = 1;

        public int RefFrames {
            get => refFrames;
            set => SetProperty(ref refFrames, value);
        }
        #endregion

        #region Filter
        private bool deblockEnable = true;

        public bool DeblockEnable {
            get => deblockEnable;
            set => SetProperty(ref deblockEnable, value);
        }

        private SampleAdaptiveOffset saoType = SampleAdaptiveOffset.Full;

        public SampleAdaptiveOffset SaoType {
            get => saoType;
            set => SetProperty(ref saoType, value);
        }
        #endregion

        #region Motion Estimation
        private IntegerMotionEstimationAlgorithm imeAlgorithm = IntegerMotionEstimationAlgorithm.HexagonBasedSearch;

        public IntegerMotionEstimationAlgorithm ImeAlgorithm {
            get => imeAlgorithm;
            set => SetProperty(ref imeAlgorithm, value);
        }

        private int fmeLevel = 4;

        public int FmeLevel {
            get => fmeLevel;
            set => SetProperty(ref fmeLevel, value);
        }

        private bool bipred;

        public bool Bipred {
            get => bipred;
            set => SetProperty(ref bipred, value);
        }

        private bool smpEnable;

        public bool SmpEnable {
            get => smpEnable;
            set => SetProperty(ref smpEnable, value);
        }

        private bool ampEnable;

        public bool AmpEnable {
            get => ampEnable;
            set => SetProperty(ref ampEnable, value);
        }

        private bool fullIntraSearch;

        public bool FullIntraSearch {
            get => fullIntraSearch;
            set => SetProperty(ref fullIntraSearch, value);
        }

        private bool tmvpEnable = true;

        public bool TmvpEnable {
            get => tmvpEnable;
            set => SetProperty(ref tmvpEnable, value);
        }
        #endregion

        #region Rate Control
        private RateControlAlgorithm rcAlgorithm = RateControlAlgorithm.NoRateControl;

        public RateControlAlgorithm RcAlgorithm {
            get => rcAlgorithm;
            set => SetProperty(ref rcAlgorithm, value);
        }

        private int targetBitrate;

        public int TargetBitrate {
            get => targetBitrate;
            set => SetProperty(ref targetBitrate, value);
        }
        #endregion

        #region Quantization Optimization
        private int rdo = 1;

        public int Rdo {
            get => rdo;
            set => SetProperty(ref rdo, value);
        }

        private bool rdoqEnable = true;

        public bool RdoqEnable {
            get => rdoqEnable;
            set => SetProperty(ref rdoqEnable, value);
        }

        private bool signhideEnable = true;

        public bool SignhideEnable {
            get => signhideEnable;
            set => SetProperty(ref signhideEnable, value);
        }

        private bool trskipEnable;

        public bool TrskipEnable {
            get => trskipEnable;
            set => SetProperty(ref trskipEnable, value);
        }

        private int intraQpOffset;

        public int IntraQpOffset {
            get => intraQpOffset;
            set => SetProperty(ref intraQpOffset, value);
        }

        private bool intraQpOffsetAuto = true;

        public bool IntraQpOffsetAuto {
            get => intraQpOffsetAuto;
            set => SetProperty(ref intraQpOffsetAuto, value);
        }

        private int vaq;

        public int Vaq {
            get => vaq;
            set => SetProperty(ref vaq, value);
        }
        #endregion

        #region Parallelism
        private int threads = -1;

        public int Threads {
            get => threads;
            set => SetProperty(ref threads, value);
        }

        private bool wpp = true;

        public bool Wpp {
            get => wpp;
            set => SetProperty(ref wpp, value);
        }
        #endregion

        #region Profile & Compliance
        private int level = 62;

        public int Level {
            get => level;
            set => SetProperty(ref level, value);
        }

        private bool highTier;

        public bool HighTier {
            get => highTier;
            set => SetProperty(ref highTier, value);
        }

        private KvazaarInterop.ScalingList scalingList = KvazaarInterop.ScalingList.Off;

        public KvazaarInterop.ScalingList ScalingList {
            get => scalingList;
            set => SetProperty(ref scalingList, value);
        }

        private int maxMerge = 5;

        public int MaxMerge {
            get => maxMerge;
            set => SetProperty(ref maxMerge, value);
        }

        private bool earlySkip = true;

        public bool EarlySkip {
            get => earlySkip;
            set => SetProperty(ref earlySkip, value);
        }
        #endregion

        #endregion
        #endregion

        public override IComponentMetadata GetMetadata() => new FileWriterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new FileWriter(pipeline) {
            Filename = Filename,
            TimestampFilename = TimestampFilename,
#if !FIXED_BIT_DEPTH
            InputBitDepth = InputBitDepth,
#endif
            ProcessRemainingBeforeStop = ProcessRemainingBeforeStop,
            // Core Quality
            Lossless = Lossless,
            QP = QP,
            // GOP Structure
            IntraPeriod = IntraPeriod,
            VpsPeriod = VpsPeriod,
            GopLen = GopLen,
            GopLowDelay = GopLowDelay,
            OpenGop = OpenGop,
            RefFrames = RefFrames,
            // Filter
            DeblockEnable = DeblockEnable,
            SaoType = SaoType,
            // Motion Estimation
            ImeAlgorithm = ImeAlgorithm,
            FmeLevel = FmeLevel,
            Bipred = Bipred,
            SmpEnable = SmpEnable,
            AmpEnable = AmpEnable,
            FullIntraSearch = FullIntraSearch,
            TmvpEnable = TmvpEnable,
            // Rate Control
            RcAlgorithm = RcAlgorithm,
            TargetBitrate = TargetBitrate,
            // Quantization Optimization
            Rdo = Rdo,
            RdoqEnable = RdoqEnable,
            SignhideEnable = SignhideEnable,
            TrskipEnable = TrskipEnable,
            IntraQpOffset = IntraQpOffset,
            IntraQpOffsetAuto = IntraQpOffsetAuto,
            Vaq = Vaq,
            // Parallelism
            Threads = Threads,
            Wpp = Wpp,
            // Profile & Compliance
            Level = Level,
            HighTier = HighTier,
            ScalingList = ScalingList,
            MaxMerge = MaxMerge,
            EarlySkip = EarlySkip,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
