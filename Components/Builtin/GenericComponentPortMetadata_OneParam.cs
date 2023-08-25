#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Builtin {
    public sealed class GenericComponentPortMetadata_OneParam : IPortMetadata {

        private readonly PropertyInfo _propertyInfo;

        private readonly Type _dataType;

        public bool IsGenericInput { get; }

        public GenericComponentPortMetadata_OneParam(PropertyInfo property, bool isGenericInput, string description) {
            _propertyInfo = property ?? throw new ArgumentNullException(nameof(property));
            IsGenericInput = isGenericInput;
            _dataType = HelperExtensions.FindPortDataType(property, PortAggregation.Object);
            Direction = HelperExtensions.FindPortDirection(property, PortAggregation.Object);
            Description = description;
            Debug.Assert(!IsGenericInput || Direction == PortDirection.Input);
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string Name => _propertyInfo.Name;

        public string? Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    if (remoteEndPointDataType?.Type is null) {
                        return false;
                    }
                    if (!IsGenericInput) {
                        return remoteEndPointDataType.Type.CanBeCastTo(_dataType);
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
            switch (Direction) {
                case PortDirection.Input:
                    if (!IsGenericInput) {
                        return _dataType;
                    }

                    if (remoteEndPointDataType?.Type is null) {
                        return null;
                    }
                    return remoteEndPointDataType.Type;
                case PortDirection.Output:
                    foreach (var type in localOtherDirectionPortsDataTypes) {
                        if (type.Type is not null) {
                            if (type.Metadata is GenericComponentPortMetadata_OneParam gMetadata && gMetadata.IsGenericInput) {
                                return type.Type;
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
            var declaringType = _propertyInfo.DeclaringType
                .GetGenericTypeDefinition()
                .MakeGenericType(typeArguments);
            var result = declaringType.GetProperty(_propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public);
            return result;
        }
    }
}
