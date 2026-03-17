#nullable enable

using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Reads an MP4 file containing HEVC video and emits AccessUnit messages.
    /// Parameter sets (VPS/SPS/PPS) are merged into the first frame's AccessUnit.
    /// </summary>
    public sealed class Mp4Demuxer : Generator, IProducer<Shared<AccessUnit>>, ISourceComponent, INotifyPropertyChanged, IDisposable {

        private const string TimestampFormat = "yyyyMMddHHmmssfffffff";

        private static readonly Regex TimestampPattern = new Regex(@"_(\d{21})$", RegexOptions.Compiled);

        private readonly CancellationTokenSource _cts = new();

        #region Ports
        public Emitter<Shared<AccessUnit>> Out { get; }
        #endregion

        #region Options

        private string filename = string.Empty;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        #region Timestamp Settings
        private StartTimeMode startTimeMode = StartTimeMode.PipelineStartTime;

        public StartTimeMode StartTimeMode {
            get => startTimeMode;
            set => SetProperty(ref startTimeMode, value);
        }

        private DateTime manualStartTime = DateTime.MinValue;

        public DateTime ManualStartTime {
            get => manualStartTime;
            set => SetProperty(ref manualStartTime, value);
        }
        #endregion

        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private Mp4DemuxerContext? context;
        private int sampleIndex;
        private long cumulativeDts;
        private List<byte[]>? parameterSets;

        public Mp4Demuxer(Pipeline pipeline) : base(pipeline) {
            Out = pipeline.CreateEmitter<Shared<AccessUnit>>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);
            if (!File.Exists(Filename)) {
                throw new FileNotFoundException($"File {Filename} does not exist.");
            }

            var startTime = StartTimeMode switch {
                StartTimeMode.PipelineStartTime => args.StartOriginatingTime,
                StartTimeMode.ParseFromFilename => ParseTimestampFromFilename(),
                StartTimeMode.Manual => ManualStartTime,
                _ => throw new InvalidOperationException($"Unknown StartTimeMode: {StartTimeMode}"),
            };

            var fileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var demuxer = new Demuxer(fileStream);

            var videoTrackIndex = -1;
            for (var i = 0u; i < demuxer.TrackCount; i++) {
                if (demuxer.GetObjectType(i) == (uint)ObjectType.HEVC) {
                    videoTrackIndex = (int)i;
                    break;
                }
            }
            if (videoTrackIndex < 0) {
                throw new InvalidOperationException("No HEVC video track found in MP4 file.");
            }

            var timescale = demuxer.GetTimescale((uint)videoTrackIndex);

            // Collect parameter sets to merge into first AccessUnit
            parameterSets = new List<byte[]>();
            for (var i = 0; ; i++) {
                var ps = demuxer.ReadParameterSet((uint)videoTrackIndex, i);
                if (ps is null) {
                    break;
                }
                parameterSets.Add(ps);
            }

            context = new Mp4DemuxerContext(startTime, fileStream, demuxer, videoTrackIndex, timescale);
        }
        #endregion

        #region ISourceComponent
        void ISourceComponent.Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            if (!ProcessRemainingBeforeStop) {
                _cts.Cancel();
            }
            Stop(finalOriginatingTime, notifyCompleted);
        }
        #endregion

        #region Generator
        protected override DateTime GenerateNext(DateTime previous) {
            if (_cts.IsCancellationRequested) {
                return DateTime.MaxValue;
            }

            var trackIdx = (uint)context!.VideoTrackIndex;
            var sampleCount = context.Demuxer.GetSampleCount(trackIdx);
            if (sampleIndex >= sampleCount) {
                return DateTime.MaxValue;
            }

            // Read sample
            var sampleSize = (int)context.Demuxer.GetSampleSize(trackIdx, (uint)sampleIndex);
            var sampleData = ArrayPool<byte>.Shared.Rent(sampleSize);
            try {
                context.Demuxer.ReadSample(trackIdx, (uint)sampleIndex, sampleData, out var pts, out var duration);

                // PTS from minimp4 (display time); DTS from cumulative duration (decode time)
                var ptsTicks = TimescaleToTicks(pts);
                var dtsTicks = TimescaleToTicks((uint)cumulativeDts);
                cumulativeDts += duration;

                // Build AccessUnit from length-prefixed sample data
                // For first frame, prepend parameter sets
                AccessUnit au;
                if (parameterSets != null && parameterSets.Count > 0) {
                    au = CreateFirstAccessUnit(sampleData, sampleSize, ptsTicks, dtsTicks);
                    parameterSets = null;
                } else {
                    au = AccessUnit.CreateFromLengthPrefixed(
                        new ReadOnlyMemory<byte>(sampleData, 0, sampleSize), ptsTicks, dtsTicks);
                }

                sampleIndex++;

                var originatingTime = context.StartTime + TimeSpan.FromTicks(dtsTicks);
                using var shared = Shared.Create(au);
                Out.Post(shared, originatingTime);

                if (sampleIndex >= sampleCount) {
                    return DateTime.MaxValue;
                }
                // Return next frame's time for Generator scheduling
                return context.StartTime + TimeSpan.FromTicks(TimescaleToTicks((uint)cumulativeDts));
            } finally {
                ArrayPool<byte>.Shared.Return(sampleData);
            }
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Create the first AccessUnit with parameter sets prepended.
        /// Builds a combined length-prefixed buffer: [PS NALs] + [sample NALs].
        /// </summary>
        private AccessUnit CreateFirstAccessUnit(byte[] sampleData, int sampleSize, long ptsTicks, long dtsTicks) {
            // Calculate total size: parameter sets as length-prefixed + sample data
            var totalSize = sampleSize;
            foreach (var ps in parameterSets!) {
                totalSize += 4 + ps.Length; // 4-byte length prefix + NAL data
            }

            var combined = ArrayPool<byte>.Shared.Rent(totalSize);
            try {
                var offset = 0;

                // Write parameter sets with length prefix
                foreach (var ps in parameterSets) {
                    combined[offset] = (byte)(ps.Length >> 24);
                    combined[offset + 1] = (byte)(ps.Length >> 16);
                    combined[offset + 2] = (byte)(ps.Length >> 8);
                    combined[offset + 3] = (byte)(ps.Length);
                    offset += 4;
                    Buffer.BlockCopy(ps, 0, combined, offset, ps.Length);
                    offset += ps.Length;
                }

                // Append original sample data
                Buffer.BlockCopy(sampleData, 0, combined, offset, sampleSize);
                offset += sampleSize;

                return AccessUnit.CreateFromLengthPrefixed(new ReadOnlyMemory<byte>(combined, 0, offset), ptsTicks, dtsTicks);
            } finally {
                ArrayPool<byte>.Shared.Return(combined);
            }
        }

        private DateTime ParseTimestampFromFilename() {
            var baseName = Path.GetFileNameWithoutExtension(Filename);
            var match = TimestampPattern.Match(baseName);
            if (!match.Success) {
                throw new InvalidOperationException($"StartTimeMode is ParseFromFilename but filename '{Path.GetFileName(Filename)}' does not contain a timestamp suffix in the expected format '_{TimestampFormat}'.");
            }
            return DateTime.ParseExact(match.Groups[1].Value, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        private long TimescaleToTicks(uint timestampInTimescale) {
            return timestampInTimescale * TimeSpan.TicksPerSecond / context!.Timescale;
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _cts.Dispose();
            context?.Dispose();
            context = null;
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
