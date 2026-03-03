using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using Minimp4Interop;

namespace OpenSense.Components.HM {
    public sealed class FileWriter<TImage> : IConsumer<Shared<TImage>>, INotifyPropertyChanged, IDisposable where TImage : ImageBase {

        private readonly object _lock = new();

        private readonly Queue<DateTime> _originatingTimeQueue = new();

        private readonly List<(byte[] Data, long PtsTicks)> _pendingNals = new();

        #region Ports
        public Receiver<Shared<TImage>> In { get; }

        public Receiver<Shared<PicYuv>> PictureIn { get; }
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

        private EncoderConfig encoderConfiguration = new EncoderConfig();

        public EncoderConfig EncoderConfiguration {
            get => encoderConfiguration;
            set => SetProperty(ref encoderConfiguration, value);
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
            PictureIn = pipeline.CreateReceiver<Shared<PicYuv>>(this, ProcessPicture, nameof(PictureIn));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            lock (_lock) {
                if (context is null) {
                    return;
                }

                /* Write buffered NALs with computed durations */
                FlushPendingNals();

                /* Flush encoder and buffer remaining output */
                var results = context.Encoder.Flush();
                foreach (var result in results) {
                    var data = new byte[result.Length];
                    Marshal.Copy(result.Data, data, 0, result.Length);
                    _pendingNals.Add((data, result.PTS));
                    result.Dispose();
                }

                /* Write any NALs that now have computable durations */
                FlushPendingNals();

                /* Write remaining NALs with minimal duration */
                FlushRemainingNals();

                /* Dispose Context */
                context.Dispose();
                context = null;
            }
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
            if (EncoderConfiguration.InputBitDepth != 16) {
                throw new InvalidOperationException($"The {nameof(In)} port requires InputBitDepth to be 16 (configured {EncoderConfiguration.InputBitDepth}), because Gray_16bpp is always 16-bit.");
            }

            var width = image.Resource.Width;
            var height = image.Resource.Height;
            lock (_lock) {
                if (disposed) {
                    return;
                }
                EnsureContext(width, height, ChromaFormat.Chroma400, EncoderConfiguration.InputBitDepth, envelope.OriginatingTime);
                _originatingTimeQueue.Enqueue(envelope.OriginatingTime);

                var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
                using var picture = new PicYuv(ChromaFormat.Chroma400, width, height);
                picture.CopyYPlane(image.Resource.UnmanagedBuffer.Data, image.Resource.UnmanagedBuffer.Size);
                EncodeAndWrite(picture, ptsInTicks);
            }
        }

        private void ProcessPicture(Shared<PicYuv> picture, Envelope envelope) {
            if (inPortActivated) {
                throw new InvalidOperationException($"Cannot use both {nameof(In)} and {nameof(PictureIn)} ports simultaneously. The {nameof(In)} port is already in use.");
            }
            pictureInPortActivated = true;

            var chromaFmt = picture.Resource.ChromaFormat;
            var width = picture.Resource.Width;
            var height = picture.Resource.Height;
            lock (_lock) {
                if (disposed) {
                    return;
                }
                EnsureContext(width, height, chromaFmt, EncoderConfiguration.InputBitDepth, envelope.OriginatingTime);
                _originatingTimeQueue.Enqueue(envelope.OriginatingTime);
                if (chromaFmt != context.Config.ChromaFormatIdc) {
                    throw new InvalidOperationException($"ChromaFormat changed: expected {context.Config.ChromaFormatIdc} (from first frame), but received {chromaFmt}.");
                }

                var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
                EncodeAndWrite(picture.Resource, ptsInTicks);
            }
        }

        private void EncodeAndWrite(PicYuv picture, long ptsInTicks) {
            FlushPendingNals();
            var results = context!.Encoder.Encode(picture, ptsInTicks);
            foreach (var result in results) {
                var data = new byte[result.Length];
                Marshal.Copy(result.Data, data, 0, result.Length);
                _pendingNals.Add((data, result.PTS));
                result.Dispose();
            }
        }

        private void FlushPendingNals() {
            while (_pendingNals.Count > 0 && _originatingTimeQueue.Count >= 2) {
                var (nalData, ptsTicks) = _pendingNals[0];
                _pendingNals.RemoveAt(0);
                var dequeuedTime = _originatingTimeQueue.Dequeue();
                var nextTime = _originatingTimeQueue.Peek();

                var durationTicks = (nextTime - dequeuedTime).Ticks;
                var duration90kHz = durationTicks > 0 ? (uint)(durationTicks * 9 / 1000) : 1u;

                var dtsTicks = (dequeuedTime - context!.StartTime).Ticks;
                var ctsOffset90kHz = (int)((ptsTicks - dtsTicks) * 9 / 1000);

                unsafe {
                    fixed (byte* ptr = nalData) {
                        context.Writer.WriteNal(new IntPtr(ptr), nalData.Length, duration90kHz, ctsOffset90kHz);
                    }
                }
            }
        }

        private void FlushRemainingNals() {
            while (_pendingNals.Count > 0) {
                var (nalData, ptsTicks) = _pendingNals[0];
                _pendingNals.RemoveAt(0);
                long dtsTicks;
                if (_originatingTimeQueue.Count > 0) {
                    dtsTicks = (_originatingTimeQueue.Dequeue() - context!.StartTime).Ticks;
                } else {
                    dtsTicks = ptsTicks;
                }
                var ctsOffset90kHz = (int)((ptsTicks - dtsTicks) * 9 / 1000);

                unsafe {
                    fixed (byte* ptr = nalData) {
                        context!.Writer.WriteNal(new IntPtr(ptr), nalData.Length, 1, ctsOffset90kHz);
                    }
                }
            }
        }

        [MemberNotNull(nameof(context))]
        private void EnsureContext(int width, int height, ChromaFormat chromaFmt, int bitDepth, DateTime originatingTime) {
            if (context is not null) {
                if (width != context.Config.SourceWidth || height != context.Config.SourceHeight) {
                    throw new InvalidOperationException($"Image size mismatch: expected {context.Config.SourceWidth}x{context.Config.SourceHeight}, got {width}x{height}. All input images must have the same dimensions.");
                }
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

            /* HM and Minimp4 */
            var config = EncoderConfiguration;
            config.SourceWidth = width;
            config.SourceHeight = height;
            config.ChromaFormatIdc = chromaFmt;
            config.InputBitDepth = bitDepth;
            if (config.InternalBitDepth <= 0) {
                config.InternalBitDepth = bitDepth;
            }
            var encoder = new Encoder(config);
            var stream = new FileStream(ActualFilename, FileMode.Create, FileAccess.Write, FileShare.Read);
            var muxer = new Muxer(stream, MuxMode.Default);
            var writer = new H26xWriter(muxer, width, height, isHEVC: true);
            context = new FileWriterContext(startTime, config, encoder, stream, muxer, writer);
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            lock (_lock) {
                if (disposed) {
                    return;
                }
                disposed = true;

                context?.Dispose();
                context = null;
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
    }
}
