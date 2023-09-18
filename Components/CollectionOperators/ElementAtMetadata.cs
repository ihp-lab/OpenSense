using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.CollectionOperators {
    [Export(typeof(IComponentMetadata))]
    public sealed class ElementAtMetadata : IComponentMetadata {

        public string Name => "Element At";

        public string Description => "Return the element at a given index. No element will be returned if the index is out of range.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = new CollectionOperatorPortMetadata[] {
            new (typeof(ElementAt<,>).GetProperty(nameof(ElementAt<short, IEnumerable<short>>.IndexIn)), false, "[Optional] The index."),
            new (typeof(ElementAt<,>).GetProperty(nameof(ElementAt<short, IEnumerable<short>>.In)), true, "[Required] The collection."),
            new (typeof(ElementAt<,>).GetProperty(nameof(ElementAt<short, IEnumerable<short>>.Out)), false, "The element."),
        };

        public ComponentConfiguration CreateConfiguration() => new ElementAtConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
