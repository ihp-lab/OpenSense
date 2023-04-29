using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenSense.Components.Contract {
    public sealed class StaticPortMetadata : IPortMetadata {

        public PropertyInfo Property { get; }

        public Type DataType { get; }

        /// <summary>
        /// This kind of port's data type is fixed. Which means it will not change according to the data type of the other side of the connection.
        /// </summary>
        /// <param name="property">The PropertyInfo of the property of the port.</param>
        /// <param name="name">Name of the port. If set to null, the name of property will be used instead.</param>
        /// <param name="description">Description of this port.</param>
        public StaticPortMetadata(PropertyInfo property, string name = null, string description = "") {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Name = name ?? property.Name;
            Description = description;
            Aggregation = HelperExtensions.FindPortAggregation(property);
            Direction = HelperExtensions.FindPortDirection(property, Aggregation);
            DataType = HelperExtensions.FindPortDataType(property, Aggregation);
        }

        #region IPortMetadata
        public object Identifier => Property.Name;

        public string Description { get; }

        public string Name { get; } 

        public PortAggregation Aggregation { get; }

        public PortDirection Direction { get; }

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            bool result;
            switch (Direction) {
                case PortDirection.Input:
                    result = remoteEndPointDataType != null && remoteEndPointDataType.CanBeCastTo(DataType);
                    return result;
                case PortDirection.Output:
                    result = remoteEndPointDataType != null && DataType.CanBeCastTo(remoteEndPointDataType);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        Type IPortMetadata.GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) => DataType;
        #endregion
    }
}
