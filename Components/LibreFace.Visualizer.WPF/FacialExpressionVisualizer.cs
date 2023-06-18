using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace.Visualizer {
    public sealed class FacialExpressionVisualizer : IConsumer<IReadOnlyDictionary<string, float>>, INotifyPropertyChanged {

        #region Ports
        public Receiver<IReadOnlyDictionary<string, float>> In { get; }

        public Emitter<string> Out { get; }
        #endregion

        public ImmutableSortedDictionary<string, float> Last { get; private set; } = ImmutableSortedDictionary<string, float>.Empty;

        public FacialExpressionVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyDictionary<string, float>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));
        }

        private void Process(IReadOnlyDictionary<string, float> FacialExpressions, Envelope envelope) {
            Last = ImmutableSortedDictionary.CreateRange(FacialExpressions);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Last)));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
