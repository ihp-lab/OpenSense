using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {

    [Serializable]
    public sealed class JoinOperatorConfiguration : OperatorConfiguration {

        public override IComponentMetadata GetMetadata() => new JoinOperatorMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var producers = GetRemoteProducers(instantiatedComponents);
            Debug.Assert(producers.Count == 2);
            var producer = Operators.Join(producers[0], producers[1], Inputs[0].DeliveryPolicy, Inputs[1].DeliveryPolicy);
            return producer;
        }
    }
}
