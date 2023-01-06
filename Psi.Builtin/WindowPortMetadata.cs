using System;
using System.Collections.Generic;
using System.Linq;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {

    public sealed class WindowPortMetadata : OperatorPortMetadata {

        public WindowPortMetadata(string name, PortDirection direction, string description = "") : base(name, direction, description) {
        }

        public override Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType;
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 1) {
                        return null;
                    }
                    var inputType = localOtherDirectionPortsDataTypes.Single();
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
