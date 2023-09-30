using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.BehaviorManagement {
    public sealed class BehaviorTree : Subpipeline, INotifyPropertyChanged, IDisposable {

        private static readonly BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly MethodInfo CreateOutputConnectorToMethod = typeof(Subpipeline).GetMethod(nameof(CreateOutputConnectorTo), Flags);

        private static readonly MethodInfo CreateInputConnectorFromMethod = typeof(Subpipeline).GetMethod(nameof(CreateInputConnectorFrom), Flags);

        private static readonly MethodInfo ConnectInputMethod = typeof(BehaviorTree).GetMethod(nameof(ConnectInput), Flags);

        private readonly IBehaviorRule _root;

        private readonly Dictionary<IPortMetadata, ConnectorInfo> _connectors = new Dictionary<IPortMetadata, ConnectorInfo>();

        private readonly Dictionary<IPortMetadata, BehaviorInputData> _inputCache = new Dictionary<IPortMetadata, BehaviorInputData>();

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
            var window = rootConfiguration.Window;
            foreach (var (port, connector) in _connectors) {
                switch (port.Direction) {
                    case PortDirection.Output:
                        //Nothing
                        break;
                    case PortDirection.Input:
                        ConnectInputMethod.MakeGenericMethod(connector.DataType).Invoke(this, new object[] { port, connector.Connector, window });
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }


            /* Instantiate */
            _root = rootConfiguration.Instantiate(serviceProvider);
        }

        internal ConnectorInfo GetConnectorInfo(IPortMetadata port) {
            var result = _connectors[port];
            return result;
        }

        private void ConnectInput<T>(IPortMetadata port, IConnector connector, TimeSpan window) {

            static object? cast(T o) {
                var result = (object?)o;
                return result;
            }

            static IReadOnlyList<Message<object?>> windowCast(IEnumerable<Message<object?>> enumerable) {                
                var result = enumerable.ToArray();//Note: The input enumerable cannot be returned directly, the serializer can not serialize it.
                return result;
            }

            void process(IReadOnlyList<Message<object?>> list, Envelope envelope) {//TODO: combine invokes from different stream that have a same timestamp, may have a littile bit delay, but save computation
                var copy = list.DeepClone();
                var input = new BehaviorInputData(port, typeof(T), copy);
                _inputCache[port] = input;//TODO: how to dispose?
                var request = new BehaviorRequest(envelope.OriginatingTime, window, _inputCache);
                var response = _root.EvaluateAsync(request, cancellationToken: default).GetAwaiter().GetResult();
                foreach (var output in response.Values) {
                    var port = output.Port;
                    dynamic connector = _connectors[port].Connector;//TODO: Convert to cached delegates so it is faster
                    connector.Out.Post((dynamic?)output.Data, output.OriginatingTime);
                }
            }

            var past = RelativeTimeInterval.Past(window, inclusive: true);
            var producer = (IProducer<T>)connector;
            producer
                .Select(cast)
                .Window(past, windowCast)
                .Do(process);
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
        public readonly struct ConnectorInfo {

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
