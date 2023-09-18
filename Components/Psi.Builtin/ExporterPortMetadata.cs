#nullable enable

using System;
using System.Collections.Generic;

namespace OpenSense.Components.Psi {
    public class ExporterPortMetadata : IPortMetadata {

        public object Identifier => Name;

        public string Name => "In";

        public string Description => "";

        public PortDirection Direction => PortDirection.Input;

        public PortAggregation Aggregation => PortAggregation.Dictionary;

        public bool CanConnectDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) => true;

        public Type? GetTransmissionDataType(RuntimePortDataType? remoteEndPointDataType, IReadOnlyList<RuntimePortDataType> localOtherDirectionPortsDataTypes, IReadOnlyList<RuntimePortDataType> localSameDirectionPortsDataTypes) => remoteEndPointDataType?.Type;
    }
}
