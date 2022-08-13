using System;
using System.Collections.Generic;
using OpenSense.Component.Contract;

namespace OpenSense.Component.PythonSupports {
    internal sealed class PythonPortMetadata : IPortMetadata {

        public Type TransmissionDataType { get; private set; }

        public PythonPortMetadata(string name, PortDirection direction, Type type, string description = null) {
            Name = name;
            Direction = direction;
            Description = description;
            TransmissionDataType = type;
        }

        #region IPortMetadata

        public object Identifier => Name;

        public string Name { get; private set; }

        public string Description { get; private set; }

        public PortDirection Direction { get; private set; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            return TransmissionDataType;
        }

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            if (remoteEndPointDataType is null) {
                return false;
            }
            var selfDataType = GetTransmissionDataType(remoteEndPointDataType, localOtherDirectionPortsDataTypes, localSameDirectionPortsDataTypes);
            if (selfDataType is null) {
                return false;
            }
            var result = selfDataType.CanBeCastTo(remoteEndPointDataType);
            return result;
        }
        #endregion
    }
}
