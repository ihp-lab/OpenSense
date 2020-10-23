using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.PipelineBuilder {
    [Serializable]
    public class PipelineConfiguration : INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Guid guid = Guid.NewGuid();

        public Guid Guid {
            get => guid;
            set => SetProperty(ref guid, value);
        }

        private string name;

        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage; // to prevent memory overflow

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        private ObservableCollection<InstanceConfiguration> instances = new ObservableCollection<InstanceConfiguration>();

        public ObservableCollection<InstanceConfiguration> Instances {
            get => instances;
            set => SetProperty(ref instances, value);
        }
    }
}
