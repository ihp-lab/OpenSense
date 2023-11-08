#nullable enable

using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Export(typeof(IComponentMetadata))]
    public sealed class DeduplicatorMetadata : IComponentMetadata {

        public string Name => "Deduplicator";

        public string Description => "Drop duplications. The default EqualityComparer will be used.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = new GenericComponentPortMetadata_OneParam[] {
            new (typeof(Deduplicator<>).GetProperty(nameof(Deduplicator<short>.In)), true, "[Required] The input stream."),
            new (typeof(Deduplicator<>).GetProperty(nameof(Deduplicator<short>.Out)), false, "The output stream. Duplications are dropped."),
        };

        public ComponentConfiguration CreateConfiguration() => new DeduplicatorConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
