using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.BehaviorManagement {
    public sealed class BehaviorTree : Subpipeline, INotifyPropertyChanged, IDisposable {

        private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly MethodInfo CreateOutputConnectorToMethod =
            typeof(BehaviorTree)
            .GetMethod(nameof(CreateOutputConnectorTo), Flags)
            ;

        private static readonly MethodInfo CreateInputConnectorFromMethod =
            typeof(BehaviorTree)
            .GetMethod(nameof(CreateInputConnectorFrom), Flags)
            ;

        private readonly IBehaviorRule _root;

        private readonly Dictionary<IPortMetadata, ConnectorInfo> _connectors = new Dictionary<IPortMetadata, ConnectorInfo>();

        public BehaviorTree(Pipeline pipeline, BehaviorRuleConfiguration rootConfiguration, IServiceProvider? serviceProvider = null, DeliveryPolicy? defaultDeliveryPolicy = null): base(pipeline, nameof(BehaviorTree), defaultDeliveryPolicy) {
            /* Create connectors */
            foreach (var port in rootConfiguration.Ports) {
                var dataType = port.GetTransmissionDataType(null, Array.Empty<RuntimePortDataType>(), Array.Empty<RuntimePortDataType>());
                Debug.Assert(dataType is not null);
                if (dataType is null) {
                    throw new Exception("Cannot decide data type.");
                }
                switch (port.Direction) {
                    case PortDirection.Output:
                        var output = (IConnector)CreateOutputConnectorToMethod
                            .MakeGenericMethod(dataType)
                            .Invoke(this, new object[] { pipeline, port.Name, } );
                        _connectors.Add(port, new ConnectorInfo(output, dataType));
                        break;
                    case PortDirection.Input:
                        var input = (IConnector)CreateInputConnectorFromMethod
                            .MakeGenericMethod(dataType)
                            .Invoke(this, new object[] { pipeline, port.Name, });
                        _connectors.Add(port, new ConnectorInfo(input, dataType));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            /* Connect */
            foreach (var (port, connector) in _connectors) {
                switch (port.Direction) {
                    case PortDirection.Output:
                        break;
                    case PortDirection.Input:
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }


            /* Instantiate */
            _root = rootConfiguration.Instantiate(serviceProvider);
        }

        internal IProducer<T> GetProducer<T>(IPortMetadata port) {
            Debug.Assert(port.Direction == PortDirection.Output);
            var connector = _connectors[port];
            var result = (Connector<T>)connector.Connector;
            return result;
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

        #region Classes
        private readonly struct ConnectorInfo {

            public IConnector Connector { get; }

            public Type DataType { get; }

            public ConnectorInfo(IConnector connector, Type dataType) {
                Connector = connector;
                DataType = dataType;
            }
        }
        #endregion
    }
}
