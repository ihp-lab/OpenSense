using System;
using System.Collections.Generic;

namespace OpenSense.Component.Contract {
    public abstract class OperatorPortMetadata : IPortMetadata {

        public OperatorPortMetadata(string name, PortDirection direction, string description = "") {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Direction = direction;
        }

        public object Identifier => Name;

        public string Name { get; private set; }

        public string Description { get; private set; }

        public PortDirection Direction { get; private set; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public virtual bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
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

        public abstract Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes);

        
    }
}
