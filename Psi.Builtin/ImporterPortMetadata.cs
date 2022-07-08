using System;
using System.Collections.Generic;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    public class ImporterPortMetadata : IPortMetadata {

        public object Identifier => Name;

        public string Name => "Out";

        public string Description => "";

        public PortDirection Direction => PortDirection.Output;

        public PortAggregation Aggregation => PortAggregation.Dictionary;

        public bool CanConnectDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) => true;

        public Type GetTransmissionDataType(Type remoteEndPointDataType, IList<Type> localOtherDirectionPortsDataTypes, IList<Type> localSameDirectionPortsDataTypes) => remoteEndPointDataType;
    }
}
