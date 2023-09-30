using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSense.Components.BehaviorManagement {
    /// <summary>
    /// Output type is the same as the input type.
    /// </summary>
    internal sealed class MirrorPortMetadata : OperatorPortMetadata {

        public MirrorPortMetadata(string name, PortDirection direction, string? description) : base(name, direction, description) {
        }

        public override Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType?.Type;//If remoteEndPointDataType is null, then return null
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 1 || localOtherDirectionPortsDataTypes.Any(t => t.Type is null)) {
                        return null;
                    }
                    return localOtherDirectionPortsDataTypes.Single().Type;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
