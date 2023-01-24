using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public sealed class JoinOperatorMetadata : IComponentMetadata {

        public string Name => "Join Operator";

        public string Description => "Joins the primary stream with values from a secondary stream.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new FusionPortMetadata("Primary", PortDirection.Input, "Primary stream."),
            new FusionPortMetadata("Secondary", PortDirection.Input, "Secondary stream."),
            new FusionPortMetadata("Out", PortDirection.Output, "Paired primary and secondary tuples."),
        };

        public ComponentConfiguration CreateConfiguration() => new JoinOperatorConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
