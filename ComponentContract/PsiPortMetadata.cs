using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Psi;

namespace OpenSense.Component.Contract {
    public abstract class PsiPortMetadata : IPortMetadata {

        public abstract object Identifier { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract PortDirection Direction { get; }

        public abstract PortAggregation Aggregation { get; }

        public virtual Type ConnectorType {
            get {
                switch (Direction) {
                    case PortDirection.Input:
                        return typeof(IConsumer<>);
                    case PortDirection.Output:
                        return typeof(IProducer<>);
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public virtual bool CanConnectConnectorType(Type remoteEndConnectorType) {
            switch (Direction) {
                case PortDirection.Input:
                    return typeof(IProducer<>).IsAssignableFrom(remoteEndConnectorType);
                case PortDirection.Output:
                    return remoteEndConnectorType.IsAssignableFrom(typeof(IConsumer<>));
                default:
                    throw new InvalidOperationException();
            }
        }

        public abstract bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes);

        public abstract Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes);
    }
}
