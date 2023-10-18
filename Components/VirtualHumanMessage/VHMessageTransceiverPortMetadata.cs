using System;
using System.Collections.Generic;

namespace OpenSense.Components.VHMessage {
    public sealed class VHMessageTransceiverPortMetadata : IPortMetadata {

        public VHMessageTransceiverPortMetadata(string name, PortDirection direction, string? description) {
            Name = name;
            Direction = direction;
            Description = description;
        }

        #region IPortMetadata
        public object Identifier => Name;

        public string Name { get; }

        public string? Description { get; }

        public PortDirection Direction { get; }

        public PortAggregation Aggregation => PortAggregation.Dictionary;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            bool result;
            switch (Direction) {
                case PortDirection.Input:
                    result = remoteEndPointDataType?.Type?.CanBeCastTo(typeof(string)) == true;
                    return result;
                case PortDirection.Output:
                    result = remoteEndPointDataType?.Type is not null && typeof(string).CanBeCastTo(remoteEndPointDataType.Type);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) {
            var result = typeof(string);
            return result;
        }
        #endregion

        
    }
}
