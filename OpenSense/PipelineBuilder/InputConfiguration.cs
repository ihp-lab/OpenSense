using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Psi;

namespace OpenSense.PipelineBuilder {
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

        private PortConfiguration input;

        public PortConfiguration Input {
            get => input;
            set => SetProperty(ref input, value);
        }

        private Guid remote;

        public Guid Remote {
            get => remote;
            set => SetProperty(ref remote, value);
        }

        private PortConfiguration output;

        public PortConfiguration Output {
            get => output;
            set => SetProperty(ref output, value);
        }

        private DeliveryPolicy deliveryPolicy;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }
    }
}
