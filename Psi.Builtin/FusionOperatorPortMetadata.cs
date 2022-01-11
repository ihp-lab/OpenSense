using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    public class FusionOperatorPortMetadata : IPortMetadata {

        public FusionOperatorPortMetadata(string name, PortDirection direction, string description = "") {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Direction = direction;
        }

        public object Identifier => Name;

        public string Name { get; private set; }

        public string Description { get; private set; }

        public PortDirection Direction { get; private set; }

        public PortAggregation Aggregation => PortAggregation.Object;

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return true;
                case PortDirection.Output:
                    var dataType = GetTransmissionDataType(remoteEndPointDataType, localOtherDirectionPortsDataTypes, localSameDirectionPortsDataTypes);
                    return remoteEndPointDataType != null && remoteEndPointDataType.IsAssignableFrom(dataType);
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
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
