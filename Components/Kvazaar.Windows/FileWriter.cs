using System;
using System.Buffers;
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
    public sealed class FileWriter<TImage> : IConsumer<Shared<TImage>>, INotifyPropertyChanged, IDisposable where TImage : ImageBase {

        #region Ports

        public Receiver<Shared<TImage>> In { get; }
        #endregion

        #region Options
        private string filename = "16bit_video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

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

        public string ActualFilename { get; private set; } = "";

        private FileWriterContext? context;

        public FileWriter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<TImage>>(this, Process, nameof(In));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            if (context is null) {
                return;
            }

            /* Flush Encoder */
            while (true) {
                var (dataChunk, frameInfo, sourcePicture, _) = context.Encoder.Encode(
                    null,
                    noDataChunk: false,
                    noFrameInfo: false,
                    noSourcePicture: false,
                    noReconstructedPicture: true
                );

                WriteEncodedFrameAndDisposeIfValid(context.Writer, dataChunk, frameInfo, sourcePicture);

                if (dataChunk is null) {
                    break;
                }
            }

            /* Dispose Context */
            context.Dispose();
            context = null;
        }
        #endregion

        private void Process(Shared<TImage> image, Envelope envelope) {
            if (image.Resource.PixelFormat != PixelFormat.Gray_16bpp) {
                throw new InvalidOperationException($"This Kvazaar only supports Gray_16bpp pixel format, but received {image.Resource.PixelFormat}.");
            }

            var width = image.Resource.Width;
            var height = image.Resource.Height;
            EnsureContext(width, height, envelope.OriginatingTime);

            // Validate image size consistency with encoder configuration
            if (width != context.Config.Width || height != context.Config.Height) {
                throw new InvalidOperationException($"Image size mismatch: expected {context.Config.Width}x{context.Config.Height}, got {width}x{height}. All input images must have the same dimensions.");
            }

            /* Create Picture */
            var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
            using var picture = new Picture(ChromaFormat.Csp400, width, height) { 
                PTS = ptsInTicks,
            };
            var imageData = image.Resource.UnmanagedBuffer.Data;
            var imageSize = image.Resource.UnmanagedBuffer.Size;
            picture.CopyYPlane(imageData, imageSize);

            /* Encode */
            var (dataChunk, frameInfo, sourcePicture, _) = context.Encoder.Encode(
                picture,
                noDataChunk: false,
                noFrameInfo: false,
                noSourcePicture: false,
                noReconstructedPicture: true
            );

            /* Write */
            WriteEncodedFrameAndDisposeIfValid(context.Writer, dataChunk, frameInfo, sourcePicture);
        }

        [MemberNotNull(nameof(context))]
        private void EnsureContext(int width, int height, DateTime originatingTime) {
            if (context is not null) {
                return;
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
                Width = width,
                Height = height,
                FramerateNumerator = 1,
                FramerateDenominator = 10_000_000, // 1 tick precision for variable frame rate
                InputFormat = InputFormat.P400,
                InputBitDepth = 16,
                Lossless = true,
            };
            var encoder = new Encoder(config);
            var stream = new FileStream(ActualFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, width, height, isHEVC: true);
            context = new FileWriterContext(startTime, config, encoder, stream, muxer, writer);

            /* Header */
            var header = encoder.GetHeaders();//Since we are writing mp4, this header only appears once at the beginning of the file.
            WriteDataChunk(writer, header, timestamp90kHz: 0); // timestamp will be ignored in this case
        }

        private static void WriteEncodedFrameAndDisposeIfValid(H26xWriter writer, DataChunk? dataChunk, FrameInfo? frameInfo, Picture? sourcePicture) {
            Debug.Assert(!(dataChunk is null ^ frameInfo is null));
            Debug.Assert(!(dataChunk is null ^ sourcePicture is null));
            if (dataChunk is null) {
                return;
            }

            // Convert PTS to 90kHz units for H.264/H.265
            // 1 tick = 100 nanoseconds, 90kHz = 90000 Hz
            // 1 second = 10,000,000 ticks = 90,000 units in 90kHz
            // So: units_90khz = ticks * 90000 / 10000000 = ticks * 9 / 1000
            var timestamp90kHz = (uint)(sourcePicture!.PTS * 9 / 1000);
            WriteDataChunk(writer, dataChunk, timestamp90kHz);

            dataChunk.Dispose();
            frameInfo!.Dispose();
            sourcePicture.Dispose();
        }

        private static void WriteDataChunk(H26xWriter writer, DataChunk dataChunk, uint timestamp90kHz) {
            Debug.Assert(dataChunk.Count > 0);
            if (dataChunk.Count == 1) {
                var (data, length) = dataChunk.Single();
                writer.WriteNal(data, length, timestamp90kHz);
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
                        writer.WriteNal(new IntPtr(ptr), dataChunk.TotalLength, timestamp90kHz);
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
