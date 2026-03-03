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

        private readonly Queue<DateTime> _originatingTimeQueue = new();

        private readonly List<(DataChunk DataChunk, FrameInfo FrameInfo, Picture SourcePicture)> _pendingFrames = new();

        #region Ports
        public Receiver<Shared<TImage>> In { get; }

        public Receiver<Shared<Picture>> PictureIn { get; }
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

        private int inputBitDepth = 16;

        public int InputBitDepth {
            get => inputBitDepth;
            set => SetProperty(ref inputBitDepth, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public string ActualFilename { get; private set; } = "";

        private FileWriterContext? context;

        private bool inPortActivated;

        private bool pictureInPortActivated;

        public FileWriter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<TImage>>(this, ProcessImage, nameof(In));
            PictureIn = pipeline.CreateReceiver<Shared<Picture>>(this, ProcessPicture, nameof(PictureIn));

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

            /* Write buffered frames with computed durations */
            FlushPendingFrames();

            /* Flush encoder and buffer remaining output */
            while (true) {
                var (dataChunk, frameInfo, sourcePicture, _) = context.Encoder.Encode(
                    null,
                    noDataChunk: false,
                    noFrameInfo: false,
                    noSourcePicture: false,
                    noReconstructedPicture: true
                );

                if (dataChunk is null) {
                    break;
                }
                _pendingFrames.Add((dataChunk, frameInfo!, sourcePicture!));
            }

            /* Write any frames that now have computable durations */
            FlushPendingFrames();

            /* Write remaining frames with minimal duration */
            FlushRemainingFrames();

            /* Dispose Context */
            context.Dispose();
            context = null;
        }
        #endregion

        private void ProcessImage(Shared<TImage> image, Envelope envelope) {
            if (pictureInPortActivated) {
                throw new InvalidOperationException($"Cannot use both {nameof(In)} and {nameof(PictureIn)} ports simultaneously. The {nameof(PictureIn)} port is already in use.");
            }
            inPortActivated = true;

            if (image.Resource.PixelFormat != PixelFormat.Gray_16bpp) {
                throw new InvalidOperationException($"The {nameof(In)} port only supports Gray_16bpp pixel format (received {image.Resource.PixelFormat}), because \\psi's PixelFormat has no multi-channel 16-bit format. For other formats, use the {nameof(PictureIn)} port.");
            }

            var width = image.Resource.Width;
            var height = image.Resource.Height;
            EnsureContext(width, height, ChromaFormat.Csp400, 16, envelope.OriginatingTime);
            ValidateDimensions(width, height);
            _originatingTimeQueue.Enqueue(envelope.OriginatingTime);

            var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
            using var picture = new Picture(ChromaFormat.Csp400, width, height) {
                PTS = ptsInTicks,
            };
            picture.CopyYPlane(image.Resource.UnmanagedBuffer.Data, image.Resource.UnmanagedBuffer.Size);
            EncodeAndWrite(picture);
        }

        private void ProcessPicture(Shared<Picture> picture, Envelope envelope) {
            if (inPortActivated) {
                throw new InvalidOperationException($"Cannot use both {nameof(In)} and {nameof(PictureIn)} ports simultaneously. The {nameof(In)} port is already in use.");
            }
            pictureInPortActivated = true;

            var chromaFmt = picture.Resource.ChromaFormat;
            var width = picture.Resource.Width;
            var height = picture.Resource.Height;
            EnsureContext(width, height, chromaFmt, InputBitDepth, envelope.OriginatingTime);
            ValidateDimensions(width, height);
            _originatingTimeQueue.Enqueue(envelope.OriginatingTime);
            if ((InputFormat)(int)chromaFmt != context.Config.InputFormat) {
                throw new InvalidOperationException($"ChromaFormat changed: expected {(ChromaFormat)(int)context.Config.InputFormat} (from first frame), but received {chromaFmt}.");
            }

            var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
            using var pic = new Picture(chromaFmt, width, height) {
                PTS = ptsInTicks,
            };
            var (yData, yLen) = picture.Resource.GetYPlane();
            pic.CopyYPlane(yData, yLen);
            if (chromaFmt != ChromaFormat.Csp400) {
                var (uData, uLen) = picture.Resource.GetUPlane();
                pic.CopyUPlane(uData, uLen);
                var (vData, vLen) = picture.Resource.GetVPlane();
                pic.CopyVPlane(vData, vLen);
            }
            EncodeAndWrite(pic);
        }

        private void EncodeAndWrite(Picture picture) {
            FlushPendingFrames();
            var (dataChunk, frameInfo, sourcePicture, _) = context!.Encoder.Encode(
                picture,
                noDataChunk: false,
                noFrameInfo: false,
                noSourcePicture: false,
                noReconstructedPicture: true
            );
            if (dataChunk is not null) {
                _pendingFrames.Add((dataChunk, frameInfo!, sourcePicture!));
            }
        }

        private void FlushPendingFrames() {
            while (_pendingFrames.Count > 0 && _originatingTimeQueue.Count >= 2) {
                var (dataChunk, frameInfo, sourcePicture) = _pendingFrames[0];
                _pendingFrames.RemoveAt(0);
                var dequeuedTime = _originatingTimeQueue.Dequeue();
                var nextTime = _originatingTimeQueue.Peek();

                var durationTicks = (nextTime - dequeuedTime).Ticks;
                var duration90kHz = durationTicks > 0 ? (uint)(durationTicks * 9 / 1000) : 1u;

                var dtsTicks = (dequeuedTime - context!.StartTime).Ticks;
                var ptsTicks = sourcePicture.PTS;
                var ctsOffset90kHz = (int)((ptsTicks - dtsTicks) * 9 / 1000);

                WriteEncodedFrameAndDisposeIfValid(context.Writer, dataChunk, frameInfo, sourcePicture, duration90kHz, ctsOffset90kHz);
            }
        }

        private void FlushRemainingFrames() {
            while (_pendingFrames.Count > 0) {
                var (dataChunk, frameInfo, sourcePicture) = _pendingFrames[0];
                _pendingFrames.RemoveAt(0);
                long dtsTicks;
                if (_originatingTimeQueue.Count > 0) {
                    dtsTicks = (_originatingTimeQueue.Dequeue() - context!.StartTime).Ticks;
                } else {
                    dtsTicks = sourcePicture.PTS;
                }
                var ptsTicks = sourcePicture.PTS;
                var ctsOffset90kHz = (int)((ptsTicks - dtsTicks) * 9 / 1000);

                WriteEncodedFrameAndDisposeIfValid(context!.Writer, dataChunk, frameInfo, sourcePicture, 1, ctsOffset90kHz);
            }
        }

        private void ValidateDimensions(int width, int height) {
            if (width != context!.Config.Width || height != context.Config.Height) {
                throw new InvalidOperationException($"Image size mismatch: expected {context.Config.Width}x{context.Config.Height}, got {width}x{height}. All input images must have the same dimensions.");
            }
        }

        [MemberNotNull(nameof(context))]
        private void EnsureContext(int width, int height, ChromaFormat chromaFmt, int bitDepth, DateTime originatingTime) {
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
                InputFormat = (InputFormat)(int)chromaFmt,
                InputBitDepth = bitDepth,
                Lossless = true,
                VpsPeriod = 0, // Emit VPS/SPS/PPS only once, that is at the beginning of the first frame. With this setting, GetHeaders() is no longer needed. 0 is the default value.
            };
            var encoder = new Encoder(config);
            var stream = new FileStream(ActualFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, width, height, isHEVC: true);
            context = new FileWriterContext(startTime, config, encoder, stream, muxer, writer);
        }

        private static void WriteEncodedFrameAndDisposeIfValid(H26xWriter writer, DataChunk dataChunk, FrameInfo frameInfo, Picture sourcePicture, uint duration90kHz, int compositionOffset90kHz) {
            WriteDataChunk(writer, dataChunk, duration90kHz, compositionOffset90kHz);

            dataChunk.Dispose();
            frameInfo.Dispose();
            sourcePicture.Dispose();
        }

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
