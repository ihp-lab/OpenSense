#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSense.Components.Psi {
    public sealed class FusionPortMetadata : OperatorPortMetadata {

        internal int Order { get; }

        public FusionPortMetadata(string name, PortDirection direction, int order, string description) : base(name, direction, description) {
            Order = order;
        }

        public override Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            switch (Direction) {
                case PortDirection.Input:
                    return remoteEndPointDataType?.Type;//If remoteEndPointDataType is null, then return null
                case PortDirection.Output:
                    if (localOtherDirectionPortsDataTypes.Count != 2 || localOtherDirectionPortsDataTypes.Any(t => t.Type is null)) {
                        return null;
                    }
                    var types = localOtherDirectionPortsDataTypes
                        .OrderBy(t => ((FusionPortMetadata)t.Metadata).Order)
                        .Select(t => t.Type)
                        .ToArray();
                    return typeof(ValueTuple<,>).MakeGenericType(types);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
