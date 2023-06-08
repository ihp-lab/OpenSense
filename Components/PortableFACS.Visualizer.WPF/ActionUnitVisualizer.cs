using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.LibreFace.Visualizer {
    public sealed class ActionUnitVisualizer : IConsumer<IReadOnlyDictionary<int, float>>, INotifyPropertyChanged {

        #region Ports
        public Receiver<IReadOnlyDictionary<int, float>> In { get; }

        public Emitter<string> Out { get; }
        #endregion

        public ImmutableSortedDictionary<int, float> Last { get; private set; } = ImmutableSortedDictionary<int, float>.Empty;

        public ActionUnitVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyDictionary<int, float>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<string>(this, nameof(Out));
        }

        private void Process(IReadOnlyDictionary<int, float> actionUnits, Envelope envelope) {
            Last = ImmutableSortedDictionary.CreateRange(actionUnits);
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
