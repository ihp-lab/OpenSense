using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;
using OpenSense.Components.Builtin;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    [Export(typeof(IComponentMetadata))]
    public sealed class NullToEmptyReplacerMetadata : IComponentMetadata {

        public string Name => "Null to Empty Replacer";

        public string Description => "Inputs are collections, and if a input is null, then replace it with an empty collection.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = new GenericComponentPortMetadata_OneParam[] {
            new (typeof(NullToEmptyReplacer<,>).GetProperty(nameof(NullToEmptyReplacer<short, IEnumerable<short>>.In)), true, "[Required] The collection."),
            new (typeof(NullToEmptyReplacer<,>).GetProperty(nameof(NullToEmptyReplacer<short, IEnumerable<short>>.Out)), false, "A collection."),
        };

        public ComponentConfiguration CreateConfiguration() => new NullToEmptyReplacerConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
