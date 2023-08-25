using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.CollectionOperators {

    public sealed class ElementAt<TElem, TCollection>
        : IConsumerProducer<TCollection, TElem>, INotifyPropertyChanged
        where TCollection : IEnumerable<TElem> {

        #region Ports
        public Receiver<int> IndexIn { get; }

        public Receiver<TCollection> In { get; }

        public Emitter<TElem> Out { get; }
        #endregion

        #region Settings
        private int index;

        public int Index {
            get => index;
            set => SetProperty(ref index, value);
        } 
        #endregion

        public ElementAt(Pipeline pipeline) {
            IndexIn = pipeline.CreateReceiver<int>(this, ProcessIndex, nameof(IndexIn));
            In = pipeline.CreateReceiver<TCollection>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<TElem>(this, nameof(Out));
        }

        private void ProcessIndex(int index, Envelope envelope) {
            Index = index;
        }

        private void Process(TCollection collection, Envelope envelope) {
            var index = Index;
            switch (collection) {
                case IReadOnlyList<TElem> readOnlyList:
                    if (0 <= index && index < readOnlyList.Count) {
                        var value = readOnlyList[index];
                        Out.Post(value, envelope.OriginatingTime);
                    }
                    break;
                case IList<TElem> list:
                    if (0 <= index && index < list.Count) {
                        var value = list[index];
                        Out.Post(value, envelope.OriginatingTime);
                    }
                    break;
                default:
                    try {
                        var value = collection.ElementAt(index);
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
