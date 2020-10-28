using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    [Serializable]
    public class InputConfiguration : INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Guid id = Guid.NewGuid();

        public Guid Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        private PortConfiguration localPort;

        /// <summary>
        /// Local input port.
        /// </summary>
        public PortConfiguration LocalPort {
            get => localPort;
            set => SetProperty(ref localPort, value);
        }

        private Guid remoteId;

        public Guid RemoteId {
            get => remoteId;
            set => SetProperty(ref remoteId, value);
        }

        private PortConfiguration remotePort;

        /// <summary>
        /// Remote output port.
        /// </summary>
        public PortConfiguration RemotePort {
            get => remotePort;
            set => SetProperty(ref remotePort, value);
        }

        private DeliveryPolicy deliveryPolicy;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }
    }
}
