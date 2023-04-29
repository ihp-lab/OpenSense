using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    public sealed class ElementAtPortMetadata : IPortMetadata {

        private readonly PropertyInfo _property;

        public ElementAtPortMetadata(PropertyInfo property, PortDirection direction, string name = null, string description = null) {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            Debug.Assert(property.PropertyType.ContainsGenericParameters);
            Debug.Assert(direction == HelperExtensions.FindPortDirection(property, HelperExtensions.FindPortAggregation(property)));
            Direction = direction;
            Name = name ?? property.Name;
            Description = description;
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string Name { get; }

        public string Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    if (remoteEndPointDataType is null) {
                        return false;
                    }
                    var elementTypes = HelperExtensions.FindElementTypesOfCollectionType(remoteEndPointDataType);
                    Debug.Assert(elementTypes is not null);
                    if (elementTypes.Count == 0) {//not a collection
                        return false;
                    }
                    if (elementTypes.Count > 1) {//multiple element types
                        return false;
                    }
                    return true;
                case PortDirection.Output:
                    if (remoteEndPointDataType is null) {
                        return false;
                    }
                    var selfDataType = GetTransmissionDataType(remoteEndPointDataType, localOtherDirectionPortsDataTypes, localSameDirectionPortsDataTypes);
                    if (selfDataType is null) {
                        /** Not able to infer output data type yet.
                         */
                        return false;
                    }
                    var result = selfDataType.CanBeCastTo(remoteEndPointDataType);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            IReadOnlyList<Type> elementTypes;
            switch (Direction) {
                case PortDirection.Input:
                    if (remoteEndPointDataType is null) {
                        return null;
                    }
                    elementTypes = HelperExtensions.FindElementTypesOfCollectionType(remoteEndPointDataType);//use remote type
                    Debug.Assert(elementTypes is not null);
                    if (elementTypes.Count == 0) {
                        return null;
                    }
                    if (elementTypes.Count > 1) {
                        return null;
                    }
                    return remoteEndPointDataType;//return collection type
                case PortDirection.Output:
                    var indexInFlag = false;
                    foreach (var type in localOtherDirectionPortsDataTypes) {
                        if (type is not null) {
                            if (indexInFlag || type != typeof(int)) {
                                elementTypes = HelperExtensions.FindElementTypesOfCollectionType(type);//use input type
                                Debug.Assert(elementTypes is not null);
                                if (elementTypes.Count == 0) {
                                    return null;
                                }
                                if (elementTypes.Count > 1) {
                                    return null;
                                }
                                return elementTypes[0];//retuen element type
                            }
                        }

                        if (type == typeof(int)) {
                            indexInFlag = true;
                        }
                    }
                    return null;
                default:
                    throw new InvalidOperationException();
            }
            
        }
        #endregion

        public PropertyInfo GetProperty(params Type[] typeArguments) {
            var declaringType = _property.DeclaringType
                .GetGenericTypeDefinition()
                .MakeGenericType(typeArguments);
            var result = declaringType.GetProperty(_property.Name, BindingFlags.Instance | BindingFlags.Public);
            return result;
        }
    }
}
