#nullable enable

using System;
using System.Collections.Generic;
using OpenSense.Components.Contract;

namespace OpenSense.Components.MediaPipe.NET {
    internal sealed class PortMetadata : IPortMetadata {

        private readonly string _name;

        private readonly Type _type;

        private readonly PortDirection _direction;

        private readonly string _description;

        public PortMetadata(string name, Type type, PortDirection direction, string description) {
            _name = name;
            _type = type;
            _direction = direction;
            _description = description;
        }

        public object Identifier => Name;

        public string Name => _name;

        public string Description => _description;

        public PortDirection Direction => _direction;

        public PortAggregation Aggregation => PortAggregation.Object;

        public Type TransmissionDataType => _type;

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
            bool result;
            switch (Direction) {
                case PortDirection.Input:
                    result = remoteEndPointDataType != null && remoteEndPointDataType.CanBeCastTo(_type);
                    return result;
                case PortDirection.Output:
                    result = remoteEndPointDataType != null && _type.CanBeCastTo(remoteEndPointDataType);
                    return result;
                default:
                    throw new InvalidOperationException();
            }
        }

        public Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) => _type;
    }
}
