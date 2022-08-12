using System;
using System.Collections.Generic;
using System.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    public sealed class StandardDeviationPortMetadata : OperatorPortMetadata {

        public StandardDeviationPortMetadata(string name, PortDirection direction, string description = "") : base(name, direction, description) {
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
                    if (!inputType.IsArray) {
                        return null;
                    }
                    var result = inputType.GetElementType();
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
