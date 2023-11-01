using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.FFMpeg {
    public sealed class FileSource
        : ISourceComponent, IProducer<Shared<Image>>, IDisposable {

        private readonly FileReader _reader;

        #region Ports
        public Emitter<Shared<Image>> Out { get; }

        public Emitter<int> CountOut { get; }
        #endregion

        #region Settings
        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private Task? task;

        private DateTime startTime;

        private int count;

        public FileSource(Pipeline pipeline, string filename) {
            _reader = new FileReader(filename, OnFrame);

            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
            CountOut = pipeline.CreateEmitter<int>(this, nameof(CountOut));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region ISourceComponent
        public void Start(Action<DateTime> notifyCompletionTime) {
            var t = Task.Run(_reader.Run);
            task = t.ContinueWith(t => {
                notifyCompletionTime(Out.LastEnvelope.OriginatingTime);
            });
        }
        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            task?.Wait();
            notifyCompleted();
        }
        #endregion

        #region Pipeline Event Handlers
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            Debug.Assert(args.StartOriginatingTime.Kind == DateTimeKind.Utc);
            startTime = args.StartOriginatingTime;
        }
        #endregion

        private (IntPtr, int, Action) OnFrame(FrameInfo info) {
            var time = startTime + info.Timestamp;
            var image = ImagePool.GetOrCreate(info.Width, info.Height, PixelFormat.BGR_24bpp);
            var ptr = image.Resource.UnmanagedBuffer.Data;
            var length = image.Resource.UnmanagedBuffer.Size;
            var action = () => {
                Out.Post(image, time);
                image.Dispose();
                count++;
                CountOut.Post(count, time);
            };
            return (ptr, length, action);
        }

        private void SafePost(Shared<Image> image, DateTime timestamp) {
            
        }

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
