using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi {
    [Export(typeof(IComponentMetadata))]
    public class JoinMetadata : IComponentMetadata {

        public string Name => "Join Operator";

        public string Description => "Joins the primary stream with values from a secondary stream.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new FusionOperatorPortMetadata("Primary", PortDirection.Input, "Primary stream."),
            new FusionOperatorPortMetadata("Secondary", PortDirection.Input, "Secondary stream."),
            new FusionOperatorPortMetadata("Out", PortDirection.Output, "Paired primary and secondary tuples."),
        };

        public ComponentConfiguration CreateConfiguration() => new JoinConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && typeof(IProducer<T>).IsAssignableFrom(instance.GetType()));
            return instance;
        }
    }
}
