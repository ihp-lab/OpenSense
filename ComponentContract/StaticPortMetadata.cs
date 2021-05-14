using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public sealed class StaticPortMetadata : IPortMetadata {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="name">Name of the port. If set to null, the name of property will be used instead.</param>
        /// <param name="description">Description of this port.</param>
        public StaticPortMetadata(PropertyInfo property, string name = null, string description = "") {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            _name = name;
            Description = description;
        }

        public PropertyInfo Property { get; private set; }

        public object Identifier => Property.Name;

        public string Description { get; private set; }

        private string _name;

        public string Name => _name ?? Property.Name;

        public PortAggregation Aggregation {
            get {
                var interfaces = Property.PropertyType.GetInterfaces();
                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))) {
                    return PortAggregation.List;
                }
                if (interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))) {
                    return PortAggregation.Dictionary;
                }
                return PortAggregation.Object;
            }
        }

        public PortDirection Direction {
            get {
                var propType = Property.PropertyType;
                static PortDirection byConsumerOrProducer(Type propType) {
                    if (propType.IsAssignableToGenericType(typeof(IConsumer<>))) {
                        return PortDirection.Input;
                    }
                    if (propType.IsAssignableToGenericType(typeof(IProducer<>))) {
                        return PortDirection.Output;
                    }
                    throw new InvalidOperationException();
                }
                switch (Aggregation) {
                    case PortAggregation.List:
                        var itemType = propType.GetGenericArguments().Single();
                        return byConsumerOrProducer(itemType);
                    case PortAggregation.Dictionary:
                        var keyType = propType.GetGenericArguments()[0];
                        var valType = propType.GetGenericArguments()[1];
                        Debug.Assert(keyType == typeof(string));
                        return byConsumerOrProducer(valType);
                    case PortAggregation.Object:
                        return byConsumerOrProducer(propType);
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public bool AcceptAnyDataType => false;

        public Type DataType {
            get {
                var propType = Property.PropertyType;
                static Type getProducerOrEmitterNestedDataType(Type propType) {
                    Debug.Assert(new[] { typeof(IConsumer<>), typeof(IProducer<>) }.Any(t => propType.IsAssignableToGenericType(t)));
                    return propType.GetGenericArguments().Single();
                }
                switch (Aggregation) {
                    case PortAggregation.List:
                        var itemType = propType.GetGenericArguments().Single();
                        return getProducerOrEmitterNestedDataType(itemType);
                    case PortAggregation.Dictionary:
                        var keyType = propType.GetGenericArguments()[0];
                        var valType = propType.GetGenericArguments()[1];
                        Debug.Assert(keyType == typeof(string));
                        return getProducerOrEmitterNestedDataType(valType);
                    case PortAggregation.Object:
                        return getProducerOrEmitterNestedDataType(propType);
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType != null && DataType.IsAssignableFrom(remoteEndPointDataType);
                case PortDirection.Output:
                    return remoteEndPointDataType != null && remoteEndPointDataType.IsAssignableFrom(DataType);
                default:
                    throw new InvalidOperationException();
            }
        }

        Type IPortMetadata.GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) => DataType;
    }
}
