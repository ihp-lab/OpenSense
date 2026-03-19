using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using KvazaarInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Minimp4Interop;

namespace OpenSense.Components.Kvazaar {
    public sealed class FileWriter : IConsumer<Shared<Picture>>, INotifyPropertyChanged, IDisposable {

        /// <summary>
        /// Minimum duration in 90kHz units, used for the last frame(s) where no subsequent frame
        /// is available to compute actual duration.
        /// </summary>
        private const uint MinDuration90kHz = 1;

        private static readonly PropertyInfo IsStoppingProperty =
            typeof(Pipeline).GetProperty("IsStopping", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMemberException(nameof(Pipeline), "IsStopping");

        private readonly Pipeline _pipeline;
        private readonly Queue<DateTime> _dtsQueue = new();
        private readonly List<DataChunk> _pendingFrames = new();

        #region Ports
        public Receiver<Shared<Picture>> In { get; }
        #endregion

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

        /// <summary>
        /// Expected input bit depth. Null = auto (defaults to Picture.MaxBitDepth at runtime).
        /// When set, used directly as config.InputBitDepth for the Kvazaar encoder.
        /// </summary>
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

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public string ActualFilename { get; private set; } = "";

        private FileWriterContext? context;

        public FileWriter(Pipeline pipeline) {
            _pipeline = pipeline;
            In = pipeline.CreateReceiver<Shared<Picture>>(this, Process, nameof(In));
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            if (context is null) {
                return;
            }

            // Write buffered frames with computed durations
            WritePendingFrames(flush: false);

            if (ProcessRemainingBeforeStop) {
                // Flush encoder remaining GOP-buffered frames
                while (EncodeOnce(null) is { } dataChunk) {
                    _pendingFrames.Add(dataChunk);
                }
            }

            // Write all remaining frames (known duration first, then min duration for the rest)
            WritePendingFrames(flush: true);

            context.Dispose();
            context = null;
        }
        #endregion

        private void Process(Shared<Picture> picture, Envelope envelope) {
            if (!ProcessRemainingBeforeStop && (bool)IsStoppingProperty.GetValue(_pipeline)!) {
                return;
            }

            // Initialize on first frame
            var chromaFmt = picture.Resource.ChromaFormat;
            var width = picture.Resource.Width;
            var height = picture.Resource.Height;
            var bitDepth = picture.Resource.BitDepth;
            EnsureContext(width, height, chromaFmt, bitDepth, envelope.OriginatingTime);

            // Per-frame validation
            if (width != context.Config.Width || height != context.Config.Height) {
                throw new InvalidOperationException($"Image size mismatch: expected {context.Config.Width}x{context.Config.Height}, got {width}x{height}. All input images must have the same dimensions.");
            }
            if ((InputFormat)(int)chromaFmt != context.Config.InputFormat) {
                throw new InvalidOperationException($"ChromaFormat changed: expected {(ChromaFormat)(int)context.Config.InputFormat} (from first frame), but received {chromaFmt}.");
            }
            if (bitDepth != context.Config.InputBitDepth) {
                throw new InvalidOperationException($"BitDepth changed: expected {context.Config.InputBitDepth} (from first frame), but received {bitDepth}.");
            }

            // Write out any buffered frames that now have computable durations
            WritePendingFrames(flush: false);

            // Encode current frame (zero-copy: encoder internally copy_refs the Picture,
            // so the native pixel buffer survives Shared<Picture> disposal after Process returns.
            // See Picture.h memory management documentation.)
            _dtsQueue.Enqueue(envelope.OriginatingTime);
            picture.Resource.PTS = (envelope.OriginatingTime - context.StartTime).Ticks;
            var dataChunk = EncodeOnce(picture.Resource);
            if (dataChunk is not null) {
                _pendingFrames.Add(dataChunk);
            }
        }

        [MemberNotNull(nameof(context))]
        private void EnsureContext(int width, int height, ChromaFormat chromaFmt, int bitDepth, DateTime originatingTime) {
            if (context is not null) {
                return;
            }

            // Validate encoder configuration
            if (InputBitDepth is { } expectedBitDepth && bitDepth != expectedBitDepth) {
                throw new InvalidOperationException($"InputBitDepth mismatch: configured {expectedBitDepth}, but first frame has BitDepth={bitDepth}.");
            }
            if (bitDepth < 8 || bitDepth > Picture.MaxBitDepth) {
                throw new InvalidOperationException($"Input Picture BitDepth must be between 8 and {Picture.MaxBitDepth}, but was {bitDepth}.");
            }
            if (!Lossless && (QP < 0 || QP > 51)) {
                throw new InvalidOperationException($"QP must be between 0 and 51, but was {QP}.");
            }
            if (IntraPeriod < 0) {
                throw new InvalidOperationException($"IntraPeriod must be non-negative, but was {IntraPeriod}.");
            }
            if (GopLen < 0) {
                throw new InvalidOperationException($"GopLen must be non-negative, but was {GopLen}.");
            }
            if (RefFrames < 1 || RefFrames > 15) {
                throw new InvalidOperationException($"RefFrames must be between 1 and 15, but was {RefFrames}.");
            }
            if (FmeLevel < 0 || FmeLevel > 4) {
                throw new InvalidOperationException($"FmeLevel must be between 0 and 4, but was {FmeLevel}.");
            }
            if (Rdo < 0 || Rdo > 2) {
                throw new InvalidOperationException($"Rdo must be between 0 and 2, but was {Rdo}.");
            }
            if (TargetBitrate < 0) {
                throw new InvalidOperationException($"TargetBitrate must be non-negative, but was {TargetBitrate}.");
            }
            if (Threads < -1 || Threads == 0) {
                throw new InvalidOperationException($"Threads must be -1 (auto) or a positive integer, but was {Threads}.");
            }
            if (MaxMerge < 1 || MaxMerge > 5) {
                throw new InvalidOperationException($"MaxMerge must be between 1 and 5, but was {MaxMerge}.");
            }

            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            var startTime = originatingTime;

            /* Filename */
            if (!TimestampFilename) {
                ActualFilename = Filename;
            } else {
                var directory = Path.GetDirectoryName(Filename);
                var baseFilename = Path.GetFileNameWithoutExtension(Filename);
                var timestamp = originatingTime.ToString("yyyyMMddHHmmssfffffff");
                var extension = Path.GetExtension(Filename);
                var newFilename = $"{baseFilename}_{timestamp}{extension}";
                ActualFilename = Path.Combine(directory ?? string.Empty, newFilename);
            }

            /* Kvazaar and Minimp4 */
            var config = new Config() {
                // Auto-detected from first frame
                Width = width,
                Height = height,
                FramerateNumerator = 1,
                FramerateDenominator = (int)TimeSpan.TicksPerSecond, // 1 tick precision for variable frame rate
                InputFormat = (InputFormat)(int)chromaFmt,
                InputBitDepth = bitDepth,
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
            };
            var encoder = new Encoder(config);
            var stream = new FileStream(ActualFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, width, height, isHEVC: true);
            context = new FileWriterContext(startTime, config, encoder, stream, muxer, writer);
        }

        /// <summary>
        /// Write pending frames to the MP4 file.
        /// When flush is false, only writes frames with computable duration (requires 2 DTS entries).
        /// When flush is true, writes all remaining frames, using MinDuration for the last ones.
        /// </summary>
        private void WritePendingFrames(bool flush) {
            while (_pendingFrames.Count > 0 && (flush || _dtsQueue.Count >= 2)) {
                using var dataChunk = _pendingFrames[0];
                _pendingFrames.RemoveAt(0);

                var duration90kHz = MinDuration90kHz;
                long dtsTicks;
                if (_dtsQueue.Count >= 2) {
                    var currentTime = _dtsQueue.Dequeue();
                    var nextTime = _dtsQueue.Peek();
                    var durationTicks = (nextTime - currentTime).Ticks;
                    if (durationTicks > 0) {
                        duration90kHz = (uint)(durationTicks * 9 / 1000);
                    }
                    dtsTicks = (currentTime - context!.StartTime).Ticks;
                } else if (_dtsQueue.Count > 0) {
                    dtsTicks = (_dtsQueue.Dequeue() - context!.StartTime).Ticks;
                } else {
                    dtsTicks = dataChunk.PresentationTimeOffset;
                }

                dataChunk.DecodingTimeOffset = dtsTicks;
                var ptsTicks = dataChunk.PresentationTimeOffset;
                var ctsOffset90kHz = (int)((ptsTicks - dtsTicks) * 9 / 1000);

                WriteDataChunk(context!.Writer, dataChunk, duration90kHz, ctsOffset90kHz);
            }
        }

        private DataChunk? EncodeOnce(Picture? picture) {
            var (dataChunk, _, _, _) = context!.Encoder.Encode(
                picture,
                noDataChunk: false,
                noFrameInfo: true,
                noSourcePicture: true,
                noReconstructedPicture: true
            );
            return dataChunk;
        }

        /// <summary>
        /// Reassembles fragmented DataChunk data into contiguous memory, then writes to minimp4.
        /// minimp4's mp4_h26x_write_nal internally uses find_nal_unit to scan the [nal, eof) range
        /// with pointer arithmetic, which requires contiguous memory. Modifying minimp4 to support
        /// fragmented storage would require significant changes to its NAL scanning logic,
        /// so we reassemble the DataChunk fragments here using MemoryPool before passing to minimp4.
        /// </summary>
        private static void WriteDataChunk(H26xWriter writer, DataChunk dataChunk, uint duration90kHz, int compositionOffset90kHz) {
            Debug.Assert(dataChunk.Count > 0);
            if (dataChunk.Count == 1) {
                var (data, length) = dataChunk.Single();
                writer.WriteNal(data, length, duration90kHz, compositionOffset90kHz);
            } else {
                using var memory = MemoryPool<byte>.Shared.Rent(dataChunk.TotalLength);
                var span = memory.Memory.Span.Slice(0, dataChunk.TotalLength);
                var offset = 0;
                foreach (var (data, length) in dataChunk) {
                    unsafe {
                        var source = new ReadOnlySpan<byte>(data.ToPointer(), length);
                        source.CopyTo(span.Slice(offset, length));
                        offset += length;
                    }
                }
                unsafe {
                    fixed (byte* ptr = span) {
                        writer.WriteNal(new IntPtr(ptr), dataChunk.TotalLength, duration90kHz, compositionOffset90kHz);
                    }
                }
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            Debug.Assert(context is null);
            context?.Dispose();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
