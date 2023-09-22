using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    public sealed class BehaviorTree : Subpipeline, INotifyPropertyChanged, IDisposable {

        private readonly IBehaviorRule _root;

        public BehaviorTree(Pipeline pipeline, IBehaviorRule root, DeliveryPolicy? defaultDeliveryPolicy = null): base(pipeline, nameof(BehaviorTree), defaultDeliveryPolicy) {
            _root = root;
            var ports = _root.Ports;
            var inputPorts = ports
                .Where(p => p.Direction == PortDirection.Input)
                .ToArray();
            var outputPorts = ports
                .Where(p => p.Direction == PortDirection.Output)
                .ToArray();
        }

        #region IDisposable
        private bool disposed;

        public override void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            base.Dispose();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
