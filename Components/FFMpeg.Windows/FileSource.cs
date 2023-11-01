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

namespace OpenSense.Components.FFMpeg {
    public sealed class FileSource
        : ISourceComponent, IProducer<int>, IDisposable {

        private readonly FileReader _reader;

        #region Ports
        public Emitter<int> Out { get; }
        #endregion

        #region Settings
        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        private Task? task;

        public FileSource(Pipeline pipeline, string filename) {
            _reader = new FileReader(filename, OnFrame);

            Out = pipeline.CreateEmitter<int>(this, nameof(Out));
        }

        #region ISourceComponent
        public void Start(Action<DateTime> notifyCompletionTime) {
            notifyCompletionTime.Invoke(DateTime.MaxValue);

            task = Task.Run(_reader.Run);
        }
        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted) {
            Debug.Assert(task!.IsCompleted);
            notifyCompleted();
        }
        #endregion

        private void OnFrame(int count) {
            var timestamp = DateTime.UtcNow;
            Out.Post(count, timestamp);
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
