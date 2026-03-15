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
using Minimp4Interop;

namespace OpenSense.Components.HM {
    public sealed class FileWriter : IConsumer<Shared<PictureSnapshot>>, INotifyPropertyChanged, IDisposable {

        private readonly object _lock = new();

        private readonly Queue<DateTime> _originatingTimeQueue = new();

        private readonly Queue<(byte[] Data, long PtsTicks)> _pendingNals = new();

        #region Ports
        public Receiver<Shared<PictureSnapshot>> In { get; }

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

        public string? ActualFilename { get; private set; }

        private FileWriterContext? context;

        public FileWriter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<PictureSnapshot>>(this, Process, nameof(In));

            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Event Handlers
        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            lock (_lock) {
                if (context is null) {
                    return;
                }

                FlushPendingNals(); // write buffered NALs with computed durations
                BufferEncoderResults(context.Encoder.Flush()); // flush encoder
                FlushPendingNals(); // write any NALs that now have computable durations
                FlushRemainingNals(); // write remaining NALs with minimal duration

                context.Dispose();
                context = null;
            }
        }
        #endregion

        private void Process(Shared<PictureSnapshot> picture, Envelope envelope) {
            var picYuv = picture.Resource.PicYuv;
            var chromaFmt = picYuv.ChromaFormat;
            var width = picYuv.Width;
            var height = picYuv.Height;
            var actualBitDepth = picture.Resource.Sps.BitDepths.Luma;
            lock (_lock) {
                if (disposed) {
                    return;
                }
                EnsureContext(width, height, chromaFmt, actualBitDepth, envelope.OriginatingTime);
                _originatingTimeQueue.Enqueue(envelope.OriginatingTime);

                var ptsInTicks = (envelope.OriginatingTime - context.StartTime).Ticks;
                EncodeAndWrite(picYuv, ptsInTicks);
            }
        }

        private void EncodeAndWrite(PictureYuv picture, long ptsInTicks) {
            FlushPendingNals();
            BufferEncoderResults(context!.Encoder.Encode(picture, ptsInTicks));
        }

        private void BufferEncoderResults(AccessUnitData[] results) {
            foreach (var result in results) {
                var data = new byte[result.Length];
                Marshal.Copy(result.Data, data, 0, result.Length);
                _pendingNals.Enqueue((data, result.PTS));
                result.Dispose();
            }
        }

        private void FlushPendingNals() {
            while (_pendingNals.Count > 0 && _originatingTimeQueue.Count >= 2) {
                var (nalData, ptsTicks) = _pendingNals.Dequeue();
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
                var (nalData, ptsTicks) = _pendingNals.Dequeue();
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
                    throw new InvalidOperationException($"Image size mismatch: expected {context.Config.SourceWidth}x{context.Config.SourceHeight}, got {width}x{height}.");
                }
                if (bitDepth != context.Config.InputBitDepth) {
                    throw new InvalidOperationException($"Bit depth changed: expected {context.Config.InputBitDepth}, got {bitDepth}.");
                }
                if (chromaFmt != context.Config.ChromaFormatIdc) {
                    throw new InvalidOperationException($"ChromaFormat changed: expected {context.Config.ChromaFormatIdc}, got {chromaFmt}.");
                }
                return;
            }

            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            var startTime = originatingTime;

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
            if (config.InputBitDepth > 0 && config.InputBitDepth != bitDepth) {
                throw new InvalidOperationException($"InputBitDepth mismatch: configured {config.InputBitDepth}, but input PictureSnapshot has {bitDepth}-bit data.");
            }
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
