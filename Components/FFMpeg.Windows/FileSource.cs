using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileSource : Generator, IProducer<Shared<Image>>, INotifyPropertyChanged, IDisposable {

        private readonly FileReader _reader;

        #region Ports
        public Emitter<Shared<Image>> Out { get; }
        #endregion

        #region Settings
        public PixelFormat PixelFormat {
            get => ToPsiPixelFormat(_reader.TargetFormat);
            set {
                var old = _reader.TargetFormat;
                var @new = ToFFMpegPixelFormat(value);
                if (old == @new) {
                    return;
                }
                _reader.TargetFormat = @new;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PixelFormat)));
            }
        }

        private bool onlyKeyFrames;

        public bool OnlyKeyFrames {
            get => onlyKeyFrames;
            set => SetProperty(ref onlyKeyFrames, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime startTime;

        private Shared<Image>? image;

        private DateTime? time;

        public FileSource(Pipeline pipeline, string filename) : base(pipeline) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException($"File {filename} does not exist.");
            }
            _reader = new FileReader(filename);

            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);
            startTime = args.StartOriginatingTime;
        }
        #endregion

        #region Generator
        protected override DateTime GenerateNext(DateTime previous) {
            bool valid;
            bool eof;
            do {
                _reader.ReadOneFrame(OnAllocate, out valid, out eof);
            } while (!valid && !eof);
            if (eof) {
                return DateTime.MaxValue;//no more data
            }
            Debug.Assert(valid);
            Debug.Assert(image is not null);
            Debug.Assert(time is not null);
            var originatingTime = (DateTime)time;
            Out.Post(image, originatingTime);
            return originatingTime;
        }

        #endregion

        private (IntPtr, int) OnAllocate(FrameInfo info) {
            if (image is not null) {
                image.Dispose();
                image = null;
            }

            if (OnlyKeyFrames && !info.KeyFrame) {
                return (IntPtr.Zero, 0);
            }

            time = startTime + info.Timestamp;
            image = ImagePool.GetOrCreate(info.Width, info.Height, PixelFormat);
            var ptr = image.Resource.UnmanagedBuffer.Data;
            var length = image.Resource.UnmanagedBuffer.Size;
            return (ptr, length);
        }

        #region Helpers
        /// <summary>
        /// Convert to AVPixelFormat enum.
        /// </summary>
        private static int ToFFMpegPixelFormat(PixelFormat psiFormat) {
            switch(psiFormat) {
                case PixelFormat.Undefined:
                    return -1;//AV_PIX_FMT_NONE
                case PixelFormat.RGB_24bpp:
                    return 2;//AV_PIX_FMT_RGB24
                case PixelFormat.BGR_24bpp:
                    return 3;//AV_PIX_FMT_BGR24
                case PixelFormat.Gray_8bpp:
                    return 8;//AV_PIX_FMT_GRAY8
                case PixelFormat.BGRA_32bpp:
                    return 28;//AV_PIX_FMT_BGRA
                case PixelFormat.Gray_16bpp:
                    if (!BitConverter.IsLittleEndian) {
                        throw new InvalidOperationException("Big endian is not supported.");
                    }
                    return 30;//AV_PIX_FMT_GRAY16LE
                case PixelFormat.RGBA_64bpp:
                    if (!BitConverter.IsLittleEndian) {
                        throw new InvalidOperationException("Big endian is not supported.");
                    }
                    return 105;//AV_PIX_FMT_RGBA64LE
                default:
                    throw new InvalidOperationException($"Unsupported \\psi pixel format {psiFormat}.");
            }
        }

        /// <summary>
        /// Convert from AVPixelFormat enum.
        /// </summary>
        private static PixelFormat ToPsiPixelFormat(int ffmpegFormat) {
            switch (ffmpegFormat) {
                case -1://AV_PIX_FMT_NONE
                    return PixelFormat.Undefined;
                case 2://AV_PIX_FMT_RGB24
                    return PixelFormat.RGB_24bpp;
                case 3://AV_PIX_FMT_BGR24
                    return PixelFormat.BGR_24bpp;
                case 8://AV_PIX_FMT_GRAY8
                    return PixelFormat.Gray_8bpp;
                case 28://AV_PIX_FMT_BGRA
                    return PixelFormat.BGRA_32bpp;
                case 30://AV_PIX_FMT_GRAY16LE
                    return PixelFormat.Gray_16bpp;
                case 105://AV_PIX_FMT_RGBA64LE
                    return PixelFormat.RGBA_64bpp;
                default:
                    throw new InvalidOperationException($"Unsupported native pixel format {ffmpegFormat}.");
            }
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            image?.Dispose();
            _reader.Dispose();
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
