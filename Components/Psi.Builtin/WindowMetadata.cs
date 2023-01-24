using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi {
    [Export(typeof(IComponentMetadata))]
    public sealed class WindowMetadata : IComponentMetadata {
        public string Name => "Window Operator";

        public string Description => "Groups signal values using a sliding window.";

        public IReadOnlyList<IPortMetadata> Ports => new[] {
            new WindowPortMetadata("In", PortDirection.Input, "Input."),
            new WindowPortMetadata("Out", PortDirection.Output, "Output."),
        };

        public ComponentConfiguration CreateConfiguration() => new WindowConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
