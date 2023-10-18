#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    public sealed class StringVisualizer : IConsumer<string?>, INotifyPropertyChanged {
        #region Ports
        public Receiver<string?> In { get; }
        #endregion

        #region Data Bindings
        private string? val;

        public string? Value {
            get => val;
            private set => SetProperty(ref val, value);
        }

        private int length;

        public int Length {
            get => length;
            private set => SetProperty(ref length, value);
        }
        #endregion

        public StringVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<string?>(this, Process, nameof(In));
        }

        private void Process(string? data, Envelope envelope) {
            Value = data;
            Length = data?.Length ?? 0;
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
