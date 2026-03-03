using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    public sealed class FileReader : Generator, INotifyPropertyChanged, IDisposable {

        private readonly string _filename;
        private Decoder? _decoder;
        private Queue<(PicYuv picture, TimeSpan timestamp)>? _frameBuffer;

        // MP4 demuxer state
        private FileStream? _fileStream;
        private Demuxer? _demuxer;
        private int _videoTrackIndex;
        private int _sampleIndex;
        private uint _timescale;

        #region Ports
        public Emitter<Shared<Image>> Out { get; }

        public Emitter<Shared<PicYuv>> PictureOut { get; }
        #endregion

        #region Options
        private bool parseFilenameTimestamp;

        public bool ParseFilenameTimestamp {
            get => parseFilenameTimestamp;
            set => SetProperty(ref parseFilenameTimestamp, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime _startTime;

        public FileReader(Pipeline pipeline, string filename) : base(pipeline) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException($"File {filename} does not exist.");
            }
            _filename = filename;

            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
            PictureOut = pipeline.CreateEmitter<Shared<PicYuv>>(this, nameof(PictureOut));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Event Handlers
        /// <summary>
        /// Timestamp format used by FileWriter: yyyyMMddHHmmssfffffff (21 characters).
        /// </summary>
        private const string TimestampFormat = "yyyyMMddHHmmssfffffff";

        private static readonly Regex TimestampPattern = new Regex(@"_(\d{21})$", RegexOptions.Compiled);

        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);
            if (ParseFilenameTimestamp) {
                var baseName = Path.GetFileNameWithoutExtension(_filename);
                var match = TimestampPattern.Match(baseName);
                if (!match.Success) {
                    throw new InvalidOperationException($"ParseFilenameTimestamp is enabled but filename '{Path.GetFileName(_filename)}' does not contain a timestamp suffix in the expected format '_yyyyMMddHHmmssfffffff'.");
                }
                _startTime = DateTime.ParseExact(match.Groups[1].Value, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
            } else {
                _startTime = args.StartOriginatingTime;
            }

            // Open MP4 file via demuxer
            _fileStream = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            _demuxer = new Demuxer(_fileStream);

            // Find HEVC video track
            _videoTrackIndex = -1;
            for (uint i = 0; i < _demuxer.TrackCount; i++) {
                if (_demuxer.GetObjectType(i) == 0x23) {
                    _videoTrackIndex = (int)i;
                    break;
                }
            }
            if (_videoTrackIndex < 0) {
                throw new InvalidOperationException("No HEVC video track found in MP4 file.");
            }

            _timescale = _demuxer.GetTimescale((uint)_videoTrackIndex);

            // Create decoder and feed VPS/SPS/PPS from MP4 container
            _decoder = new Decoder();
            FeedParameterSets();

            _sampleIndex = 0;
            _frameBuffer = new Queue<(PicYuv picture, TimeSpan timestamp)>();
        }
        #endregion

        /// <summary>
        /// Feed VPS/SPS/PPS extracted from HEVC DSI to the decoder.
        /// </summary>
        private void FeedParameterSets() {
            Debug.Assert(_demuxer is not null);
            Debug.Assert(_decoder is not null);

            for (int i = 0; ; i++) {
                var parameterSet = _demuxer.ReadParameterSet((uint)_videoTrackIndex, i);
                if (parameterSet is null) {
                    break;
                }
                _decoder.FeedNal(parameterSet);
            }
        }

        /// <summary>
        /// Parse length-prefixed NAL units from an MP4 sample and feed each to the decoder.
        /// MP4 samples contain NAL units in 4-byte big-endian length prefix format.
        /// </summary>
        private PicYuv[] FeedSampleToDecoder(byte[] sampleData) {
            var allFrames = new List<PicYuv>();
            int offset = 0;
            while (offset + 4 <= sampleData.Length) {
                int nalSize = (sampleData[offset] << 24) | (sampleData[offset + 1] << 16)
                            | (sampleData[offset + 2] << 8) | sampleData[offset + 3];
                offset += 4;
                if (nalSize <= 0 || offset + nalSize > sampleData.Length) {
                    break;
                }

                var nalData = new byte[nalSize];
                Buffer.BlockCopy(sampleData, offset, nalData, 0, nalSize);
                offset += nalSize;

                var frames = _decoder!.FeedNal(nalData);
                allFrames.AddRange(frames);
            }
            return allFrames.ToArray();
        }

        /// <summary>
        /// Convert a timestamp from track timescale units to .NET TimeSpan.
        /// </summary>
        private TimeSpan TimescaleToTimeSpan(uint timestampInTimescale) {
            // Convert to .NET Ticks: timestamp / timescale * 10_000_000
            // Use long arithmetic to avoid overflow
            var ticks = (long)timestampInTimescale * TimeSpan.TicksPerSecond / _timescale;
            return TimeSpan.FromTicks(ticks);
        }

        #region Generator
        protected override DateTime GenerateNext(DateTime previous) {
            Debug.Assert(_frameBuffer is not null);
            Debug.Assert(_decoder is not null);
            Debug.Assert(_demuxer is not null);

            // Pull frames from decoder until we have at least one
            while (_frameBuffer.Count == 0) {
                var sampleCount = _demuxer.GetSampleCount((uint)_videoTrackIndex);
                if (_sampleIndex >= sampleCount) {
                    // EOF: flush remaining B-frames from decoder
                    var remaining = _decoder.FlushAndCollect();
                    if (remaining.Length == 0) {
                        return DateTime.MaxValue;
                    }
                    foreach (var pic in remaining) {
                        // Flushed frames use the last known timestamp as approximation
                        _frameBuffer.Enqueue((pic, previous - _startTime));
                    }
                    break;
                }

                // Read next sample from MP4
                var sampleData = _demuxer.ReadSample(
                    (uint)_videoTrackIndex,
                    (uint)_sampleIndex,
                    out var timestamp,
                    out var duration
                );
                _sampleIndex++;

                // Feed length-prefixed NAL units to decoder
                var frames = FeedSampleToDecoder(sampleData);
                var ts = TimescaleToTimeSpan(timestamp);
                foreach (var pic in frames) {
                    _frameBuffer.Enqueue((pic, ts));
                }
            }

            var (picture, frameTimestamp) = _frameBuffer.Dequeue();
            var originatingTime = _startTime + frameTimestamp;

            // Post to Out if subscribed and format is Chroma400.
            // Psi's PixelFormat has no multi-channel 16-bit format (only Gray_16bpp),
            // so multi-channel YUV (420/422/444) cannot be represented as a Psi Image.
            if (Out.HasSubscribers && picture.ChromaFormat == ChromaFormat.Chroma400) {
                var pixelData = picture.GetYPlaneAs16Bit();
                using var image = ImagePool.GetOrCreate(picture.Width, picture.Height, PixelFormat.Gray_16bpp);
                Debug.Assert(image.Resource.UnmanagedBuffer.Size == pixelData.Length);
                image.Resource.UnmanagedBuffer.CopyFrom(pixelData);
                Out.Post(image, originatingTime);
            }

            // Post to PictureOut (Shared takes ownership of picture lifecycle)
            using var sharedPicture = Shared.Create(picture);
            PictureOut.Post(sharedPicture, originatingTime);

            return originatingTime;
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            if (_frameBuffer is not null) {
                while (_frameBuffer.Count > 0) {
                    _frameBuffer.Dequeue().picture.Dispose();
                }
                _frameBuffer = null;
            }

            _decoder?.Dispose();
            _decoder = null;

            _demuxer?.Dispose();
            _demuxer = null;

            _fileStream?.Dispose();
            _fileStream = null;
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
