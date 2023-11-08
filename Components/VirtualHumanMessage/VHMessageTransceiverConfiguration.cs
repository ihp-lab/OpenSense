using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.VHMessage {
    [Serializable]
    public sealed class VHMessageTransceiverConfiguration : ComponentConfiguration {

        private static readonly VHMessageTransceiverMetadata Metadata = new VHMessageTransceiverMetadata();

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => Metadata;

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) { 
            var result = new VHMessageTransceiver(pipeline);
            foreach (var inputConfig in Inputs) {
                var remoteEnvironment = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var producer = HelperExtensions.GetProducer<string?>(remoteEnvironment, inputConfig.RemotePort);
                Debug.Assert(producer is not null);

                var index = (string)inputConfig.LocalPort.Index;
                if (index is null) {
                    throw new ArgumentException("Index is null.", nameof(inputConfig));
                }
                var receiver = result.AddInput(index);

                producer.PipeTo(receiver, inputConfig.DeliveryPolicy);
            }
            return result;
        }
        #endregion
    }
}
