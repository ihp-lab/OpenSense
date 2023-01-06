using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Serializable]
    public sealed class StandardDeviationConfiguration : OperatorConfiguration {

        public override IComponentMetadata GetMetadata() => new StandardDeviationMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var producers = GetRemoteProducers(instantiatedComponents);
            Debug.Assert(producers.Count == 1);
            var producer = Operators.Std(producers.Single(), Inputs.Single().DeliveryPolicy);
            return producer;
        }
    }
}
