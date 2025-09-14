using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using KvazaarInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using Minimp4Interop;

namespace OpenSense.Components.Kvazaar {
    public sealed class FileWriter<TImage> : INotifyPropertyChanged, IConsumer<Shared<TImage>>, IDisposable where TImage : ImageBase {

        private readonly string _filename;

        #region Ports
        public Receiver<Shared<TImage>> In { get; }
        #endregion

        #region Options
        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime? startTime;

        private Resources? resources;

        public FileWriter(Pipeline pipeline, string filename) {
            _filename = filename;

            In = pipeline.CreateReceiver<Shared<TImage>>(this, Process, nameof(In));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            //TODO: close but not dispose
        }
        #endregion

        private void Process(Shared<TImage> image, Envelope envelope) {
            // Assert that the image is Gray_16bpp format
            Debug.Assert(image.Resource.PixelFormat == PixelFormat.Gray_16bpp, "FileWriter only supports Gray_16bpp pixel format");

            var width = image.Resource.Width;
            var height = image.Resource.Height;

            EnsureInitialized(width, height, envelope.OriginatingTime);

            // Calculate PTS in ticks relative to start time
            var ptsInTicks = (envelope.OriginatingTime - startTime.Value).Ticks;

            // Create picture and copy Gray16 data
            using var picture = new Picture(ChromaFormat.Csp400, width, height) { 
                PTS = ptsInTicks,
            };

            // Copy image data to Y plane (grayscale)
            var imageData = image.Resource.UnmanagedBuffer.Data;
            var imageSize = image.Resource.UnmanagedBuffer.Size;
            picture.CopyYPlane(imageData, imageSize);

            // Encode the picture
            var dataChunk = resources.Encoder.Encode(picture);
            if (dataChunk is not null) {
                // Convert PTS to 90kHz units for H.264/H.265
                // 1 tick = 100 nanoseconds, 90kHz = 90000 Hz
                // 1 second = 10,000,000 ticks = 90,000 units in 90kHz
                // So: units_90khz = ticks * 90000 / 10000000 = ticks * 9 / 1000
                var timestamp90kHz = (uint)(ptsInTicks * 9 / 1000);
                WriteDataChunk(dataChunk, timestamp90kHz);
            }
        }

        [MemberNotNull(nameof(startTime), nameof(resources))]
        private void EnsureInitialized(int width, int height, DateTime originatingTime) {
            if (resources is not null) {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
                return;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
            }

            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            startTime = originatingTime;

            string actualFilename;
            if (!TimestampFilename) {
                actualFilename = _filename;
            } else {
                var directory = Path.GetDirectoryName(_filename);
                var baseFilename = Path.GetFileNameWithoutExtension(_filename);
                var timestamp = originatingTime.ToString("yyyyMMddHHmmssfffffff");
                var extension = Path.GetExtension(_filename);
                var newFilename = $"{baseFilename}_{timestamp}{extension}";
                actualFilename = Path.Combine(directory ?? string.Empty, newFilename);
            }

            var config = new Config() {
                Width = width,
                Height = height,
                FramerateNumerator = 1,
                FramerateDenominator = 10_000_000, // 1 tick precision for variable frame rate
                InputFormat = InputFormat.P400,
                InputBitDepth = 16,
                Lossless = true,
            };
            var encoder = new Encoder(config);
            var stream = new FileStream(actualFilename, FileMode.Create, FileAccess.Write);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, width, height, isHEVC: true);
            resources = new Resources(config, encoder, stream, muxer, writer);
        }

        private void WriteDataChunk(DataChunk dataChunk, uint timestamp90kHz) {
            Debug.Assert(resources is not null);
            Debug.Assert(dataChunk.Count() == 1);

            // Iterate through the DataChunk to get NAL units
            foreach (var (data, length) in dataChunk) {
                resources.Writer.WriteNal(data, length, timestamp90kHz);
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            if (resources is not null) {

                // Flush encoder
                var chunk = resources.Encoder.Encode(null);
                if (chunk is not null) {
                    Debug.Assert(false, "We have not handle B-frame yet, this branch should never execute");
                    WriteDataChunk(chunk, 0);//Should not be 0
                }

                resources.Dispose();
            }
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

        #region Classes
        private sealed class Resources : IDisposable {

            public Config Config { get; }

            public Encoder Encoder { get; }

            public FileStream Stream { get; }

            public Muxer Muxer { get; }

            public H26xWriter Writer { get; }

            public Resources(Config config, Encoder encoder, FileStream stream, Muxer muxer, H26xWriter writer) {
                Config = config;
                Encoder = encoder;
                Stream = stream;
                Muxer = muxer;
                Writer = writer;
            }

            #region IDisposable
            private bool disposed;

            public void Dispose() {
                if (disposed) {
                    return;
                }
                disposed = true;

                Encoder.Dispose();
                Config.Dispose();

                Writer.Dispose();
                Muxer.Dispose();
                Stream.Dispose();
            }
            #endregion
        }
        #endregion
    }
}
