#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenSense.Components {
    /// <summary>
    /// Port metadata for generic components where the port accepts Wrapper&lt;T&gt; with T constrained to a base type.
    /// For example, a port accepting Shared&lt;T&gt; where T : ImageBase.
    /// </summary>
    public sealed class ConstrainedGenericPortMetadata : IPortMetadata {

        private readonly PropertyInfo _propertyInfo;
        private readonly Type _wrapperGenericDefinition;
        private readonly Type _innerBaseType;

        /// <param name="propertyInfo">PropertyInfo from the open generic type, e.g. typeof(FileWriter&lt;&gt;).GetProperty("In")</param>
        /// <param name="wrapperGenericDefinition">The wrapper generic type definition, e.g. typeof(Shared&lt;&gt;)</param>
        /// <param name="innerBaseType">Base type constraint for the inner type, e.g. typeof(ImageBase)</param>
        /// <param name="direction">Port direction</param>
        /// <param name="description">Port description</param>
        public ConstrainedGenericPortMetadata(
            PropertyInfo propertyInfo,
            Type wrapperGenericDefinition,
            Type innerBaseType,
            PortDirection direction,
            string? description = null
        ) {
            _propertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
            _wrapperGenericDefinition = wrapperGenericDefinition ?? throw new ArgumentNullException(nameof(wrapperGenericDefinition));
            _innerBaseType = innerBaseType ?? throw new ArgumentNullException(nameof(innerBaseType));
            Direction = direction;
            Description = description;
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string Name => _propertyInfo.Name;

        public string? Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            if (remoteEndPointDataType?.Type is null) {
                return false;
            }

            var remoteType = remoteEndPointDataType.Type;
            if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != _wrapperGenericDefinition) {
                return false;
            }

            var innerType = remoteType.GetGenericArguments()[0];
            return _innerBaseType.IsAssignableFrom(innerType);
        }

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            if (remoteEndPointDataType?.Type is null) {
                return null;
            }

            var remoteType = remoteEndPointDataType.Type;
            if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != _wrapperGenericDefinition) {
                return null;
            }

            var innerType = remoteType.GetGenericArguments()[0];
            if (!_innerBaseType.IsAssignableFrom(innerType)) {
                return null;
            }

            return remoteType;
        }
        #endregion

        /// <summary>
        /// Constructs the PropertyInfo for the closed generic type with the given type arguments.
        /// </summary>
        public PropertyInfo GetConstructedPropertyInfo(params Type[] typeArguments) {
            var declaringType = _propertyInfo.DeclaringType!
                .GetGenericTypeDefinition()
                .MakeGenericType(typeArguments);
            return declaringType.GetProperty(_propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public)!;
        }
    }
}
