using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public sealed class StaticPortMetadata : IPortMetadata {

        public StaticPortMetadata(string name, PropertyInfo property) {
            _name = name;
            if (property is null) {
                throw new ArgumentNullException(nameof(property));
            }
            Property = property;
        }

        public StaticPortMetadata(PropertyInfo property) : this(null, property) {}

        public PropertyInfo Property { get; private set; }

        public object Identifier => Property.Name;

        public string Description { get; set; } = "";

        private string _name;

        public string Name { 
            get => _name ?? Property.Name;
        }

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

        private static bool IsAssignableToGenericType(Type givenType, Type genericType) {
            var interfaceTypes = givenType.GetInterfaces();
            foreach (var it in interfaceTypes) {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType) {
                    return true;
                }
            }
            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType) {
                return true;
            }
            var baseType = givenType.BaseType;
            if (baseType == null) {
                return false;
            }
            return IsAssignableToGenericType(baseType, genericType);
        }

        public PortDirection Direction {
            get {
                var propType = Property.PropertyType;
                static PortDirection byConsumerOrProducer(Type propType) {
                    if (IsAssignableToGenericType(propType, typeof(IConsumer<>))) {
                        return PortDirection.Input;
                    }
                    if (IsAssignableToGenericType(propType, typeof(IProducer<>))) {
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

        public Type DataType {
            get {
                var propType = Property.PropertyType;
                static Type getProducerOrEmitterNestedDataType(Type propType) {
                    Debug.Assert(new[] { typeof(IConsumer<>), typeof(IProducer<>) }.Any(t => IsAssignableToGenericType(propType.GetGenericTypeDefinition(), t)));
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

        Type IPortMetadata.DataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            return DataType;
        }
    }
}
