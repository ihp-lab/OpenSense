using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.SerialPorts {
    public sealed class SerialPortPinPuller : INotifyPropertyChanged, IDisposable {

        private readonly SerialPort _port;

        #region Ports
        public Receiver<bool> DtrIn { get; }

        public Emitter<bool> DtrOut { get; }

        public Receiver<bool> RtsIn { get; }

        public Emitter<bool> RtsOut { get; }
        #endregion

        #region Settings

        private bool pullUpDtrOnStart = false;

        public bool PullUpDtrOnStart {
            get => pullUpDtrOnStart;
            set => SetProperty(ref pullUpDtrOnStart, value);
        }

        private bool dtrUseSourceOriginatingTime = false;

        public bool DtrUseSourceOriginatingTime {
            get => dtrUseSourceOriginatingTime;
            set => SetProperty(ref dtrUseSourceOriginatingTime, value);
        }

        private bool pullUpRtsOnStart = false;

        public bool PullUpRtsOnStart {
            get => pullUpRtsOnStart;
            set => SetProperty(ref pullUpRtsOnStart, value);
        }

        private bool rtsUseSourceOriginatingTime = false;

        public bool RtsUseSourceOriginatingTime {
            get => rtsUseSourceOriginatingTime;
            set => SetProperty(ref rtsUseSourceOriginatingTime, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region States
        public string PortName => _port.PortName;
        #endregion

        public SerialPortPinPuller(
            Pipeline pipeline,
            string portName
        ) {
            _port = new SerialPort() {
                PortName = portName,
                Handshake = Handshake.None,//Manual RTS control
            };

            DtrIn = pipeline.CreateReceiver<bool>(this, ProcessDtr, nameof(DtrIn));
            DtrOut = pipeline.CreateEmitter<bool>(this, nameof(DtrOut));
            RtsIn = pipeline.CreateReceiver<bool>(this, ProcessRTS, nameof(RtsIn));
            RtsOut = pipeline.CreateEmitter<bool>(this, nameof(RtsOut));

            pipeline.PipelineRun += OnPipelineRun;
            pipeline.PipelineCompleted += OnPipelineCompleted;
        }

        #region Pipeline Events
        private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
            _port.Open();

            Debug.Assert(!_port.DtrEnable);
            if (PullUpDtrOnStart) {
                _port.DtrEnable = true;
                var timestamp = DtrUseSourceOriginatingTime ? args.StartOriginatingTime : DtrOut.Pipeline.GetCurrentTime();
                DtrOut.Post(true, timestamp);
            }

            Debug.Assert(!_port.RtsEnable);
            if (PullUpRtsOnStart) {
                _port.RtsEnable = true;
                var timestamp = RtsUseSourceOriginatingTime ? args.StartOriginatingTime : RtsOut.Pipeline.GetCurrentTime();
                RtsOut.Post(true, timestamp);
            }
        }

        private void OnPipelineCompleted(object? sender, PipelineCompletedEventArgs args) {
            _port.Close();
        }
        #endregion

        private void ProcessDtr(bool value, Envelope envelope) {
            if (_port.DtrEnable == value) {
                return;
            }
            _port.DtrEnable = value;
            var timestamp = DtrUseSourceOriginatingTime ? envelope.OriginatingTime : DateTime.UtcNow;
            DtrOut.Post(value, timestamp);
        }

        private void ProcessRTS(bool value, Envelope envelope) {
            if (_port.RtsEnable == value) {
                return;
            }
            _port.RtsEnable = value;
            var timestamp = RtsUseSourceOriginatingTime ? envelope.OriginatingTime : DateTime.UtcNow;
            RtsOut.Post(value, timestamp);
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            _port.Dispose();
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
