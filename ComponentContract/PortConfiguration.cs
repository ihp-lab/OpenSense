using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenSense.Components {

    [Serializable]
    public class PortConfiguration : INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private object identifier;

        public object Identifier {
            get => identifier;
            set => SetProperty(ref identifier, value);
        }

        private object index;

        public object Index {
            get => index;
            set => SetProperty(ref index, value);
        }
    }
}
