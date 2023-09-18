#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenSense.Components.Builtin;

namespace OpenSense.Components.CollectionOperators {
    public sealed class CollectionOperatorPortMetadata : IPortMetadata {

        private readonly PropertyInfo _propertyInfo;

        private readonly Type _dataType;

        public bool IsGenericCollectionInput { get; }

        public CollectionOperatorPortMetadata(PropertyInfo property, bool isGenericCollectionInput, string description) {
            _propertyInfo = property ?? throw new ArgumentNullException(nameof(property));
            IsGenericCollectionInput = isGenericCollectionInput;
            _dataType = HelperExtensions.FindPortDataType(property, PortAggregation.Object);
            Direction = HelperExtensions.FindPortDirection(property, PortAggregation.Object);
            Description = description;
            Debug.Assert(!IsGenericCollectionInput || Direction == PortDirection.Input);
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string Name => _propertyInfo.Name;

        public string Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    if (remoteEndPointDataType?.Type is null) {
                        return false;
                    }
                    if (!IsGenericCollectionInput) {
                        return remoteEndPointDataType.Type.CanBeCastTo(_dataType);
                    }

                    var elementTypes = HelperExtensions.FindElementTypesOfCollectionType(remoteEndPointDataType.Type);
                    Debug.Assert(elementTypes is not null);
                    if (elementTypes.Count == 0) {//not a collection
                        return false;
                    }
                    if (elementTypes.Count > 1) {//multiple element types
                        return false;
                    }
                    return true;

                case PortDirection.Output:
                    if (remoteEndPointDataType?.Type is null) {
                        return false;
                    }
                    var selfDataType = GetTransmissionDataType(remoteEndPointDataType, localOtherDirectionPortsDataTypes, localSameDirectionPortsDataTypes);
                    if (selfDataType is null) {
                        /** Not able to infer output data type yet.
                         */
                        return false;
                    }
                    var result = selfDataType.CanBeCastTo(remoteEndPointDataType.Type);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            IReadOnlyList<Type> elementTypes;
            switch (Direction) {
                case PortDirection.Input:
                    if (!IsGenericCollectionInput) {
                        return _dataType;
                    }

                    if (remoteEndPointDataType?.Type is null) {
                        return null;
                    }
                    elementTypes = HelperExtensions.FindElementTypesOfCollectionType(remoteEndPointDataType.Type);//use remote type
                    Debug.Assert(elementTypes is not null);
                    if (elementTypes.Count == 0) {
                        return null;
                    }
                    if (elementTypes.Count > 1) {
                        return null;
                    }
                    return remoteEndPointDataType.Type;//return collection type
                case PortDirection.Output:
                    foreach (var type in localOtherDirectionPortsDataTypes) {
                        if (type.Type is not null) {
                            if (type.Metadata is GenericComponentPortMetadata_OneParam gMetadata && gMetadata.IsGenericInput) {
                                return type.Type;
                            }
                            if (type.Metadata is CollectionOperatorPortMetadata cMetadata && cMetadata.IsGenericCollectionInput) {
                                elementTypes = HelperExtensions.FindElementTypesOfCollectionType(type.Type);
                                Debug.Assert(elementTypes is not null);
                                if (elementTypes.Count == 0) {
                                    return null;
                                }
                                if (elementTypes.Count > 1) {
                                    return null;
                                }
                                return elementTypes[0];
                            }
                        }
                        
                    }
                    return null;
                default:
                    throw new InvalidOperationException();
            }
        }
        #endregion

        public PropertyInfo GetConstructedPropertyInfo(params Type[] typeArguments) {
            var declaringType = _propertyInfo.DeclaringType!
                .GetGenericTypeDefinition()
                .MakeGenericType(typeArguments);
            var result = declaringType.GetProperty(_propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
            return result!;
        }
    }
}
