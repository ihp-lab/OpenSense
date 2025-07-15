using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Common;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileSource : Generator, INotifyPropertyChanged, IDisposable {

        private readonly FileReader _reader;

        #region Ports
        public Emitter<Shared<Frame>> FrameOut { get; }

        public Emitter<Shared<Image>> ImageOut { get; }
        #endregion

        #region Settings
        public Microsoft.Psi.Imaging.PixelFormat TargetFormat {
            get => ToPsiPixelFormat(_reader.TargetFormat);
            set {
                var old = _reader.TargetFormat;
                var @new = ToFFMpegPixelFormat(value);
                if (old == @new) {
                    return;
                }
                _reader.TargetFormat = @new;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetFormat)));
            }
        }

        public int TargetWidth {
            get => _reader.TargetWidth;
            set {
                var old = _reader.TargetWidth;
                if (old == value) {
                    return;
                }
                _reader.TargetWidth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetWidth)));
            }
        }

        public int TargetHeight {
            get => _reader.TargetHeight;
            set {
                var old = _reader.TargetHeight;
                if (old == value) {
                    return;
                }
                _reader.TargetHeight = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetHeight)));
            }
        }

        public bool OnlyKeyFrames {
            get => _reader.OnlyKeyFrames;
            set {
                var old = _reader.OnlyKeyFrames;
                if (old == value) {
                    return;
                }
                _reader.OnlyKeyFrames = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnlyKeyFrames)));
            }
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime startTime;

        public FileSource(Pipeline pipeline, string filename) : base(pipeline) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException($"File {filename} does not exist.");
            }
            _reader = new FileReader(filename);

            FrameOut = pipeline.CreateEmitter<Shared<Frame>>(this, nameof(FrameOut));
            ImageOut = pipeline.CreateEmitter<Shared<Image>>(this, nameof(ImageOut));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);
            startTime = args.StartOriginatingTime;

            /* Validate pixel format compatibility at runtime */
            if (TargetFormat != Microsoft.Psi.Imaging.PixelFormat.Undefined) {
                var ffmpegFormat = ToFFMpegPixelFormat(TargetFormat);
                if (ffmpegFormat == FFMpegInterop.PixelFormat.None) {
                    throw new InvalidOperationException($"Pixel format {TargetFormat} is not supported by FFMpeg interop.");
                }
            }
        }
        #endregion

        #region Generator
        protected override DateTime GenerateNext(DateTime previous) {
            if (!_reader.MoveNext()) {
                return DateTime.MaxValue; // End of stream
            }
            var frame = _reader.Current;
            if (frame is null) {
                return DateTime.MaxValue; // End of stream
            }
            using var sharedFrame = Shared.Create(frame);
            var originatingTime = startTime + frame.Timestamp;
            FrameOut.Post(sharedFrame, originatingTime);
            if (ImageOut.HasSubscribers) {// Only post images when necessary, as it incurs computational cost
                var psiPixelFormat = ToPsiPixelFormat(frame.Format);
                if (psiPixelFormat == Microsoft.Psi.Imaging.PixelFormat.Undefined) {
                    var buffer = UnmanagedBuffer.CreateCopyFrom(frame.Data);//Unnecessary copy, but no way to avoid it.
                    var img = new Image(buffer, frame.Width, frame.Height, stride: 0, psiPixelFormat);
                    using var image = Shared.Create(img);
                    ImageOut.Post(image, originatingTime);
                } else {
                    using var image = ImagePool.GetOrCreate(frame.Width, frame.Height, psiPixelFormat);
                    Trace.Assert(image.Resource.UnmanagedBuffer.Size > 0);
                    Trace.Assert(image.Resource.Size == frame.Data.Length);
                    Marshal.Copy(frame.Data, 0, image.Resource.UnmanagedBuffer.Data, image.Resource.UnmanagedBuffer.Size);
                    ImageOut.Post(image, originatingTime);
                }
            }
            return originatingTime;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Convert from FFMpeg PixelFormat to Psi PixelFormat.
        /// </summary>
        private static Microsoft.Psi.Imaging.PixelFormat ToPsiPixelFormat(FFMpegInterop.PixelFormat ffmpegFormat) {
            switch (ffmpegFormat) {
                case FFMpegInterop.PixelFormat.None:
                    return Microsoft.Psi.Imaging.PixelFormat.Undefined;
                case FFMpegInterop.PixelFormat.RGB24:
                    return Microsoft.Psi.Imaging.PixelFormat.RGB_24bpp;
                case FFMpegInterop.PixelFormat.BGR24:
                    return Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp;
                case FFMpegInterop.PixelFormat.Gray8:
                    return Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp;
                case FFMpegInterop.PixelFormat.BGRA:
                    return Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp;
                case FFMpegInterop.PixelFormat.Gray16LE:
                    return Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp;
                case FFMpegInterop.PixelFormat.RGBA64LE:
                    return Microsoft.Psi.Imaging.PixelFormat.RGBA_64bpp;
                default:
                    return Microsoft.Psi.Imaging.PixelFormat.Undefined;
            }
        }

        /// <summary>
        /// Convert from Psi PixelFormat to FFMpeg PixelFormat.
        /// </summary>
        private static FFMpegInterop.PixelFormat ToFFMpegPixelFormat(Microsoft.Psi.Imaging.PixelFormat psiFormat) {
            switch (psiFormat) {
                case Microsoft.Psi.Imaging.PixelFormat.Undefined:
                    return FFMpegInterop.PixelFormat.None;
                case Microsoft.Psi.Imaging.PixelFormat.RGB_24bpp:
                    return FFMpegInterop.PixelFormat.RGB24;
                case Microsoft.Psi.Imaging.PixelFormat.BGR_24bpp:
                    return FFMpegInterop.PixelFormat.BGR24;
                case Microsoft.Psi.Imaging.PixelFormat.Gray_8bpp:
                    return FFMpegInterop.PixelFormat.Gray8;
                case Microsoft.Psi.Imaging.PixelFormat.BGRA_32bpp:
                    return FFMpegInterop.PixelFormat.BGRA;
                case Microsoft.Psi.Imaging.PixelFormat.Gray_16bpp:
                    if (!BitConverter.IsLittleEndian) {
                        throw new InvalidOperationException("Big endian is not supported.");
                    }
                    return FFMpegInterop.PixelFormat.Gray16LE;
                case Microsoft.Psi.Imaging.PixelFormat.RGBA_64bpp:
                    if (!BitConverter.IsLittleEndian) {
                        throw new InvalidOperationException("Big endian is not supported.");
                    }
                    return FFMpegInterop.PixelFormat.RGBA64LE;
                default:
                    // For unsupported formats, return None (no conversion)
                    return FFMpegInterop.PixelFormat.None;
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
