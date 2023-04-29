using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.CollectionOperators {

    public sealed class ElementAt<T> : IConsumerProducer<IEnumerable<T>, T>, INotifyPropertyChanged {

        private static readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;

        #region Ports
        public Receiver<IEnumerable<T>> In { get; }

        public Receiver<int> IndexIn { get; }

        public Emitter<T> Out { get; }
        #endregion

        #region Settings
        private int index;

        public int Index {
            get => index;
            set => SetProperty(ref index, value);
        } 
        #endregion

        public ElementAt(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IEnumerable<T>>(this, Process, nameof(In));
            IndexIn = pipeline.CreateReceiver<int>(this, ProcessIndex, nameof(IndexIn));
            Out = pipeline.CreateEmitter<T>(this, nameof(Out));
        }

        private void ProcessIndex(int index, Envelope envelope) {
            Index = index;
        }

        private void Process(IEnumerable<T> values, Envelope envelope) {
            var index = Index;
            switch (values) {
                case IReadOnlyList<T> list:
                    if (0 <= index && index < list.Count) {
                        var value = list[index];
                        Out.Post(value, envelope.OriginatingTime);
                    }
                    break;
                default:
                    try {
                        var value = values.ElementAt(index);
                        Out.Post(value, envelope.OriginatingTime);//Note: ElementAtOrDefault() is not suitable here.
                    } catch (ArgumentOutOfRangeException) {
                        ;//Nothing
                    }
                    break;
            }
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
