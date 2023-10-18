using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using Microsoft.Psi;

namespace OpenSense.Components.VHMessage {
    [Export(typeof(IComponentMetadata))]
    public sealed class VHMessageTransceiverMetadata : IComponentMetadata {

        private static readonly VHMessageTransceiverPortMetadata[] SharedPorts = new[] {
            new VHMessageTransceiverPortMetadata("In", PortDirection.Input, "Leave the index empty to send the message as it is, otherwise it will be used as a prefix for filtering."),
            new VHMessageTransceiverPortMetadata("Out", PortDirection.Output, "Leave the index empty to receive all messages, otherwise it will be used as a filter to filter prefixes."),
        };

        #region IComponentMetadata
        public string Name => "VH Message Transceiver";

        public string Description => "Send and receive VH Messages to interop with Virtual Human Toolkit. ActiveMQ service should be available at tcp://localhost:61616.";

        public IReadOnlyList<IPortMetadata> Ports => SharedPorts;

        public ComponentConfiguration CreateConfiguration() => new VHMessageTransceiverConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(portConfiguration.Identifier, "Out"));
            var obj = (VHMessageTransceiver)instance;
            var index = (string?)portConfiguration.Index;
            if (index is null) {
                throw new ArgumentException("Index is null.", nameof(portConfiguration));
            }
            var producer = obj.AddOutput(index);
            var result = HelperExtensions.CastProducerResult<T>(producer);
            return result;
        }
        #endregion
    }
}
