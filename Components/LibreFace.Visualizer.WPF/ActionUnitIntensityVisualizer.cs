using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace.Visualizer {
    public sealed class ActionUnitIntensityVisualizer : IConsumer<IReadOnlyDictionary<string, float>>, INotifyPropertyChanged {

        #region Ports
        public Receiver<IReadOnlyDictionary<string, float>> In { get; }
        #endregion

        public IReadOnlyDictionary<string, float> Last { get; private set; } = ImmutableSortedDictionary<string, float>.Empty;

        public ActionUnitIntensityVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyDictionary<string, float>>(this, Process, nameof(In));
        }

        private void Process(IReadOnlyDictionary<string, float> actionUnits, Envelope envelope) {
            Last = actionUnits;
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
