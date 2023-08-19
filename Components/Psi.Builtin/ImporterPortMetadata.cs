#nullable enable

using System;
using System.Collections.Generic;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    public class ImporterPortMetadata : IPortMetadata {

        public object Identifier => Name;

        public string Name => "Out";

        public string Description => "";

        public PortDirection Direction => PortDirection.Output;

        public PortAggregation Aggregation => PortAggregation.Dictionary;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) => true;

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) => remoteEndPointDataType?.Type;
    }
}
