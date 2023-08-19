#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenSense.Components.Contract {
    public sealed class StaticPortMetadata : IPortMetadata {

        internal bool IsMadeFromPropertyInfo { get; }

        public Type DataType { get; }

        /// <summary>
        /// This kind of port's data type is fixed. Which means it will not change according to the data type of the other side of the connection.
        /// </summary>
        /// <param name="property">The PropertyInfo of the property of the port.</param>
        /// <param name="name">Name of the port. If set to null, the name of property will be used instead.</param>
        /// <param name="description">Description of this port.</param>
        public StaticPortMetadata(PropertyInfo property, string? description = null) {
            IsMadeFromPropertyInfo = true;
            Name = property.Name;
            Description = description;
            Aggregation = HelperExtensions.FindPortAggregation(property);
            Direction = HelperExtensions.FindPortDirection(property, Aggregation);
            DataType = HelperExtensions.FindPortDataType(property, Aggregation);
        }

        public StaticPortMetadata(string name, PortDirection direction, PortAggregation aggregation, Type dataType, string? description = null) { 
            Name = name;
            Direction = direction;
            Aggregation = aggregation;
            DataType = dataType;
            Description = description;
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string? Description { get; }

        public string Name { get; } 

        public PortAggregation Aggregation { get; }

        public PortDirection Direction { get; }

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            bool result;
            switch (Direction) {
                case PortDirection.Input:
                    result = remoteEndPointDataType?.Type is not null && remoteEndPointDataType.Type.CanBeCastTo(DataType);
                    return result;
                case PortDirection.Output:
                    result = remoteEndPointDataType?.Type is not null && DataType.CanBeCastTo(remoteEndPointDataType.Type);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) => DataType;
        #endregion
    }
}
