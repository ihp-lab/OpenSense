using System;
using System.Collections.Generic;
using System.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Operator {
    public class FusionOperatorPortMetadata : PsiPortMetadata {

        public FusionOperatorPortMetadata(string name, PortDirection direction, string description = "") {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _description = description;
            _direction = direction;
        }

        public override object Identifier => Name;

        private string _name;

        public override string Name => _name;

        private string _description;

        public override string Description => _description;

        private PortDirection _direction;

        public override PortDirection Direction => _direction;

        public override PortAggregation Aggregation => PortAggregation.Object;

        public override bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) {
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
