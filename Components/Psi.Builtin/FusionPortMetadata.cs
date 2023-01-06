using System;
using System.Collections.Generic;
using System.Linq;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    public sealed class FusionPortMetadata : OperatorPortMetadata {

        public FusionPortMetadata(string name, PortDirection direction, string description = "") : base(name, direction, description) {
        }

        public override Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType;
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 2 || localOtherDirectionPortsDataTypes.Contains(null)) {
                        return null;
                    }
                    return typeof(ValueTuple<,>).MakeGenericType(localOtherDirectionPortsDataTypes.ToArray());
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
