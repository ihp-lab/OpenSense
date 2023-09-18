#nullable enable

using System;
using System.Collections.Generic;

namespace OpenSense.Components {
    public abstract class OperatorPortMetadata : IPortMetadata {

        public OperatorPortMetadata(string name, PortDirection direction, string? description = null) {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Direction = direction;
            Description = description;
        }

        public object Identifier => Name;

        public string Name { get; }

        public string? Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public virtual bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
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

        public abstract Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes);

    }
}
