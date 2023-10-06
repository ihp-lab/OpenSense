using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.Psi {

    [Serializable]
    public sealed class JoinOperatorConfiguration : ComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new JoinOperatorMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var metadata = GetMetadata();
            var inputs = metadata
                .InputPorts()
                .Cast<FusionPortMetadata>()
                .ToArray();
            var producers = this.GetRemoteProducerMappings(instantiatedComponents)
                .OrderBy(m => inputs.Single(i => Equals(i.Name, m.InputConfiguration.LocalPort.Identifier)).Order)//We need to order the inputs, because it may be out of order.
                .Select(m => m.Producer)
                .ToArray();
            Debug.Assert(producers.Length == 2);
            var producer = Operators.Join(producers[0], producers[1], Inputs[0].DeliveryPolicy, Inputs[1].DeliveryPolicy);
            return producer;
        }
    }
}
