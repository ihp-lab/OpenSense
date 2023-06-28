using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace.Visualizer {
    public sealed class ActionUnitPresenceVisualizer : IConsumer<IReadOnlyDictionary<string, bool>>, INotifyPropertyChanged {

        #region Ports
        public Receiver<IReadOnlyDictionary<string, bool>> In { get; }
        #endregion

        public IReadOnlyDictionary<string, bool> Last { get; private set; } = ImmutableSortedDictionary<string, bool>.Empty;

        public ActionUnitPresenceVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyDictionary<string, bool>>(this, Process, nameof(In));
        }

        private void Process(IReadOnlyDictionary<string, bool> actionUnits, Envelope envelope) {
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
