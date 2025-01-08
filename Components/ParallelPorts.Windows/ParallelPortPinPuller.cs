using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.ParallelPorts {
    public sealed class ParallelPortPinPuller : IConsumer<byte>, IProducer<byte>, INotifyPropertyChanged {

        #region Ports
        public Receiver<byte> In { get; }

        public Emitter<byte> Out { get; }
        #endregion

        #region Options
        private short memoryAddress = 0x378;

        /// <remarks>
        /// Inpoutx64 uses signed short for memory address. Here we follow its convention.
        /// </remarks>
        public short MemoryAddress {
            get => memoryAddress;
            set => SetProperty(ref memoryAddress, value);
        }

        private bool setOnStart = false;

        public bool SetOnStart {
            get => setOnStart;
            set => SetProperty(ref setOnStart, value);
        }

        private byte setOnStartValue = 0;

        public byte SetOnStartValue {
            get => setOnStartValue;
            set => SetProperty(ref setOnStartValue, value);
        }

        private bool useSourceOriginatingTime = false;

        public bool UseSourceOriginatingTime {
            get => useSourceOriginatingTime;
            set => SetProperty(ref useSourceOriginatingTime, value);
        }

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public ParallelPortPinPuller(Pipeline pipeline) {
            In = pipeline.CreateReceiver<byte>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<byte>(this, nameof(Out));

            pipeline.PipelineRun += OnPipelineRun;
        }

        #region Pipeline Events
        private void OnPipelineRun(object? sender, PipelineRunEventArgs e) {
            if (!Inpoutx64PInvoke.IsXP64Bit()) {
                throw new Exception("Inpoutx64 is x64 only.");
            }
            if (!Inpoutx64PInvoke.IsInpOutDriverOpen()) {
                throw new Exception("Inpoutx64 driver is not open. Please install the Inpoutx64 driver from its official website.");
            }

            if (!SetOnStart) {
                return;
            }
            Inpoutx64PInvoke.Out32(MemoryAddress, SetOnStartValue);
            var timestamp = UseSourceOriginatingTime ? e.StartOriginatingTime : Out.Pipeline.GetCurrentTime();
            Out.Post(SetOnStartValue, timestamp);
        }
        #endregion

        private void Process(byte value, Envelope envelope) {
            Inpoutx64PInvoke.Out32(MemoryAddress, value);
            var timestamp = UseSourceOriginatingTime ? envelope.OriginatingTime : Out.Pipeline.GetCurrentTime();
            Out.Post(value, timestamp);
        }

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
