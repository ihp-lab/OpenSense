using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenSense.PipelineBuilder {
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

        private string propertyName;

        public string PropertyName {
            get => propertyName;
            set => SetProperty(ref propertyName, value);
        }

        private string indexer;

        public string Indexer {
            get => indexer;
            set => SetProperty(ref indexer, value);
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case PortConfiguration other:
                    return PropertyName == other.PropertyName && Indexer == other.Indexer;
                default:
                    return false;
            }
        }
    }
}
