using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {

    [Serializable]
    public class JoinConfiguration : FusionOperatorConfiguration {

        public override IComponentMetadata GetMetadata() => new JoinMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents) {
            var producers = GetRemoteProducers(instantiatedComponents);
            Debug.Assert(producers.Count == 2);
            var producer = Operators.Join(producers[0], producers[1], Inputs[0].DeliveryPolicy, Inputs[1].DeliveryPolicy);
            return producer;
        }
    }
}
