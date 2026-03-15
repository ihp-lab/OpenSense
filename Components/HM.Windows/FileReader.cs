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
    public sealed class FileReader : Generator, ISourceComponent, INotifyPropertyChanged, IDisposable {

        /// <summary>
        /// Timestamp format used by FileWriter: yyyyMMddHHmmssfffffff (21 characters).
        /// </summary>
        private const string TimestampFormat = "yyyyMMddHHmmssfffffff";

        private static readonly Regex TimestampPattern = new Regex(@"_(\d{21})$", RegexOptions.Compiled);

        private readonly string _filename;
        private readonly Queue<(PictureSnapshot picture, TimeSpan timestamp)> _frameBuffer = new();
        private readonly PriorityQueue<TimeSpan, TimeSpan> _ptsQueue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly List<PictureSnapshot> _decodedFrames = new();

        #region Ports
        public Emitter<Shared<PictureSnapshot>> Out { get; }
        #endregion

        #region Options

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

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public SequenceParameterSetSnapshot? SequenceParameterSetSnapshot { get; private set; }

        private FileReaderContext? context;
        private int sampleIndex;
        private bool spsPosted;

        public FileReader(Pipeline pipeline, string filename) : base(pipeline) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException($"File {filename} does not exist.");
            }
            _filename = filename;

            Out = pipeline.CreateEmitter<Shared<PictureSnapshot>>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);

            var startTime = StartTimeMode switch {
                StartTimeMode.PipelineStartTime => args.StartOriginatingTime,
                StartTimeMode.ParseFromFilename => ParseTimestampFromFilename(),
                StartTimeMode.Manual => ManualStartTime,
                _ => throw new InvalidOperationException($"Unknown StartTimeMode: {StartTimeMode}"),
            };

            // Open MP4 file via demuxer
            var fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            var demuxer = new Demuxer(fileStream);

            // Find HEVC video track
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

            // Create decoder and feed VPS/SPS/PPS from MP4 container
            var decoder = new Decoder();
            var initFrames = new List<PictureSnapshot>();
            for (var i = 0; ; i++) {
                var parameterSet = demuxer.ReadParameterSet((uint)videoTrackIndex, i);
                if (parameterSet is null) {
                    break;
                }
                decoder.FeedNal(parameterSet, -1, initFrames);
            }
            foreach (var pic in initFrames) {
                pic.Dispose();
            }

            context = new FileReaderContext(startTime, fileStream, demuxer, videoTrackIndex, timescale, decoder);
        }

        #endregion

        #region ISourceComponent

        /// <summary>
        /// Re-implementation of <see cref="ISourceComponent.Stop"/> to cancel the decoding loop
        /// before Generator's Stop sets internal flags. Without this, GenerateNext would remain
        /// blocked in EnsureFrameBuffered, preventing the pipeline from completing.
        /// </summary>
        void ISourceComponent.Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            _cts.Cancel();
            Stop(finalOriginatingTime, notifyCompleted);
        }

        #endregion

        #region Generator

        /// <summary>
        /// Decode samples until the frame buffer has at least one frame.
        /// Returns true if a frame is available, false if EOF or cancelled.
        /// </summary>
        private bool EnsureFrameBuffered() {
            while (_frameBuffer.Count == 0) {
                if (_cts.IsCancellationRequested) {
                    return false;
                }
                var sampleCount = context!.Demuxer.GetSampleCount((uint)context!.VideoTrackIndex);
                if (sampleIndex >= sampleCount) {
                    // EOF: flush remaining B-frames
                    _decodedFrames.Clear();
                    context.Decoder.FlushAndCollect(_decodedFrames);
                    if (_decodedFrames.Count == 0) {
                        return false;
                    }
                    foreach (var pic in _decodedFrames) {
                        _frameBuffer.Enqueue((pic, _ptsQueue.Dequeue()));
                    }
                    return true;
                }

                // Read next sample from MP4
                var trackIdx = (uint)context!.VideoTrackIndex;
                var sampleSize = (int)context.Demuxer.GetSampleSize(trackIdx, (uint)sampleIndex);
                var sampleData = ArrayPool<byte>.Shared.Rent(sampleSize);
                try {
                    context.Demuxer.ReadSample(trackIdx, (uint)sampleIndex, sampleData, out var timestamp, out var duration);
                    sampleIndex++;

                    // Record sample PTS for display-order matching
                    var pts = TimescaleToTimeSpan(timestamp);
                    _ptsQueue.Enqueue(pts, pts);

                    // Feed NAL units to decoder; assign PTS from min-heap to each output frame
                    FeedSampleToDecoder(sampleData, sampleSize);
                } finally {
                    ArrayPool<byte>.Shared.Return(sampleData);
                }
            }
            return true;
        }

        protected override DateTime GenerateNext(DateTime previous) {
            if (!EnsureFrameBuffered()) {
                return DateTime.MaxValue;
            }

            var (picture, frameTimestamp) = _frameBuffer.Dequeue();
            var originatingTime = context!.StartTime + frameTimestamp;

            if (!spsPosted) {
                spsPosted = true;
                SequenceParameterSetSnapshot = picture.Sps;
            }

            using var shared = Shared.Create(picture);
            Out.Post(shared, originatingTime);

            // Return next frame's timestamp, or DateTime.MaxValue if no more frames
            if (!EnsureFrameBuffered()) {
                return DateTime.MaxValue;
            }
            return context.StartTime + _frameBuffer.Peek().timestamp;
        }
        #endregion

        #region Decoding

        private DateTime ParseTimestampFromFilename() {
            var baseName = Path.GetFileNameWithoutExtension(_filename);
            var match = TimestampPattern.Match(baseName);
            if (!match.Success) {
                throw new InvalidOperationException($"StartTimeMode is ParseFromFilename but filename '{Path.GetFileName(_filename)}' does not contain a timestamp suffix in the expected format '_{TimestampFormat}'.");
            }
            return DateTime.ParseExact(match.Groups[1].Value, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        /// <summary>
        /// Parse length-prefixed NAL units from an MP4 sample and feed each to the decoder.
        /// Decoded pictures are enqueued in <see cref="_frameBuffer"/>.
        /// </summary>
        private void FeedSampleToDecoder(byte[] sampleData, int sampleSize) {
            var offset = 0;
            while (offset + 4 <= sampleSize) {
                var nalSize = (sampleData[offset] << 24) | (sampleData[offset + 1] << 16)
                            | (sampleData[offset + 2] << 8) | sampleData[offset + 3];
                offset += 4;
                if (nalSize <= 0 || offset + nalSize > sampleSize) {
                    break;
                }

                var nalData = ArrayPool<byte>.Shared.Rent(nalSize);
                try {
                    Buffer.BlockCopy(sampleData, offset, nalData, 0, nalSize);
                    offset += nalSize;

                    _decodedFrames.Clear();
                    context!.Decoder.FeedNal(nalData, nalSize, _decodedFrames);
                    foreach (var pic in _decodedFrames) {
                        _frameBuffer.Enqueue((pic, _ptsQueue.Dequeue()));
                    }
                } finally {
                    ArrayPool<byte>.Shared.Return(nalData);
                }
            }
        }

        /// <summary>
        /// Convert a timestamp from track timescale units to .NET TimeSpan.
        /// </summary>
        private TimeSpan TimescaleToTimeSpan(uint timestampInTimescale) {
            // Convert to .NET Ticks: timestamp / timescale * 10_000_000
            // Use long arithmetic to avoid overflow
            var ticks = (long)timestampInTimescale * TimeSpan.TicksPerSecond / context!.Timescale;
            return TimeSpan.FromTicks(ticks);
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _cts.Cancel();
            _cts.Dispose();

            while (_frameBuffer.Count > 0) {
                _frameBuffer.Dequeue().picture.Dispose();
            }

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
