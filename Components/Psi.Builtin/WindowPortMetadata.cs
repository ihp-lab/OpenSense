#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSense.Components.Psi {

    public sealed class WindowPortMetadata : OperatorPortMetadata {

        public WindowPortMetadata(string name, PortDirection direction, string? description = null) : base(name, direction, description) {
        }

        public override Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType?.Type;
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 1) {
                        return null;
                    }
                    var inputType = localOtherDirectionPortsDataTypes.Single().Type;
                    if (inputType is null) {
                        return null;
                    }
                    var result = inputType.MakeArrayType();
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
