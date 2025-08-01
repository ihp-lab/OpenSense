using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileWriter : INotifyPropertyChanged, IDisposable {

        private readonly FFMpegInterop.FileWriter _writer;

        #region Ports
        public Receiver<Shared<Frame>> FrameIn { get; }

        public Receiver<Shared<Image>> ImageIn { get; }
        #endregion

        #region Settings
        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private DateTime? startTime;

        public FileWriter(Pipeline pipeline, string filename, int targetWidth, int targetHeight) {
            _writer = new (filename, targetWidth, targetHeight);

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
            startTime ??= envelope.OriginatingTime;

            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var clone = new Frame(pts, frame.Resource);
            _writer.WriteFrame(clone);
        }

        private void ProcessImage(Shared<Image> image, Envelope envelope) {
            startTime ??= envelope.OriginatingTime;

            var pts = (envelope.OriginatingTime - (DateTime)startTime).Ticks;
            using var frame = CreateFrame(pts, image.Resource);
            _writer.WriteFrame(frame);
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
