using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileWriter : INotifyPropertyChanged, IDisposable {

        private readonly FFMpegInterop.FileWriter _writer = new();

        #region Ports
        public Receiver<Shared<Frame>> FrameIn { get; }

        public Receiver<Shared<Image>> ImageIn { get; }
        #endregion

        #region Settings
        public string Filename {
            get => _writer.Filename;
            set => _writer.Filename = value;
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        public FFMpegInterop.PixelFormat TargetFormat {
            get => _writer.TargetFormat;
            set => _writer.TargetFormat = value;
        }

        public int TargetWidth {
            get => _writer.TargetWidth;
            set => _writer.TargetWidth = value;
        }

        public int TargetHeight {
            get => _writer.TargetHeight;
            set => _writer.TargetHeight = value;
        }

        public int GopSize {
            get => _writer.GopSize;
            set => _writer.GopSize = value;
        }

        public int MaxBFrames {
            get => _writer.MaxBFrames;
            set => _writer.MaxBFrames = value;
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime? startTime;

        public FileWriter(Pipeline pipeline) {
            FrameIn = pipeline.CreateReceiver<Shared<Frame>>(this, ProcessFrame, nameof(FrameIn));
            ImageIn = pipeline.CreateReceiver<Shared<Image>>(this, ProcessImage, nameof(ImageIn));

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

        private void ProcessFrame(Shared<Frame> frame, Envelope envelope) {
            SaveStartTimeIfFirstFrame(envelope.OriginatingTime);
            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var clone = new Frame(pts, frame.Resource);
            _writer.WriteFrame(clone);
        }

        private void ProcessImage(Shared<Image> image, Envelope envelope) {
            SaveStartTimeIfFirstFrame(envelope.OriginatingTime);
            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var frame = CreateFrame(pts, image.Resource);
            _writer.WriteFrame(frame);
        }

        [MemberNotNull(nameof(startTime))]
        private void SaveStartTimeIfFirstFrame(DateTime originatingTime) {
            if (startTime is not null) {
                return;
            }
            Debug.Assert(originatingTime.Kind == DateTimeKind.Utc);
            startTime = originatingTime;

            if (!TimestampFilename) {
                return;
            }
            var directory = Path.GetDirectoryName(_writer.Filename);
            var baseFilename = Path.GetFileNameWithoutExtension(_writer.Filename);
            var timestamp = originatingTime.ToString("yyyyMMddHHmmssfffffff");
            var extension = Path.GetExtension(_writer.Filename);
            var newFilename = $"{baseFilename}_{timestamp}{extension}";
            _writer.Filename = Path.Combine(directory ?? string.Empty, newFilename);
        }

        private static Frame CreateFrame(long pts, Image image) {
            var width = image.Width;
            var height = image.Height;
            var format = image.PixelFormat.ToFFMpegPixelFormat();
            var data = image.UnmanagedBuffer.Data;
            var length = image.UnmanagedBuffer.Size;
            var result = new Frame(pts, width, height, format, data, length);
            return result;
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _writer.Dispose();
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
