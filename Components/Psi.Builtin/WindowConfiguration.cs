using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.Psi {
    [Serializable]
    public sealed class WindowConfiguration : ComponentConfiguration {

        private RelativeTimeInterval relativeTimeInterval = RelativeTimeInterval.Empty;

        public RelativeTimeInterval RelativeTimeInterval {
            get => relativeTimeInterval;
            set => SetProperty(ref relativeTimeInterval, value);
        }

        public override IComponentMetadata GetMetadata() => new WindowMetadata();

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider serviceProvider) {
            var producers = this.GetRemoteProducers(instantiatedComponents);
            Debug.Assert(producers.Count == 1);
            var producer = Operators.Window(producers.Single(), RelativeTimeInterval, Inputs.Single().DeliveryPolicy);
            return producer;
        }
    }
}
