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
    public sealed class FileSource : Generator, INotifyPropertyChanged, IDisposable {

        private readonly FileReader _reader;

        #region Ports
        public Emitter<Shared<Frame>> FrameOut { get; }

        public Emitter<Shared<Image>> ImageOut { get; }
        #endregion

        #region Settings
        public Microsoft.Psi.Imaging.PixelFormat TargetFormat {
            get => _reader.TargetFormat.ToPsiPixelFormat();
            set {
                var old = _reader.TargetFormat;
                var @new = value.ToFFMpegPixelFormat();
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
                var ffmpegFormat = TargetFormat.ToFFMpegPixelFormat();
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
                var psiPixelFormat = frame.Format.ToPsiPixelFormat();
                if (psiPixelFormat == Microsoft.Psi.Imaging.PixelFormat.Undefined) {
                    throw new NotSupportedException($"Pixel format {frame.Format} is not supported by \\psi. Either set the {nameof(TargetFormat)} to a supported format, or disconnect subscribers from the {nameof(ImageOut)} emitter.");
                }
                Debug.Assert(frame.PlaneCount == 1);
                var (data, stride, length) = frame.GetPlaneBuffer(0);
                Trace.Assert(length == frame.Height * stride);
                using var image = ImagePool.GetOrCreate(frame.Width, frame.Height, psiPixelFormat);
                Trace.Assert(image.Resource.UnmanagedBuffer.Size > 0);
                Trace.Assert(image.Resource.Size == length);
                image.Resource.UnmanagedBuffer.CopyFrom(data, length);
                ImageOut.Post(image, originatingTime);
            }
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
