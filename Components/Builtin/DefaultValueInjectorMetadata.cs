using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Export(typeof(IComponentMetadata))]
    public sealed class DefaultValueInjectorMetadata : IComponentMetadata {

        public string Name => "Default Value Injector";

        public string Description => "Pass-through data from the input stream to the output stream. If a timestamp is missing in the input stream, it outputs a default value with that timestamp.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = new GenericComponentPortMetadata_OneParam[] {
            new (typeof(DefaultValueInjector<>).GetProperty(nameof(DefaultValueInjector<short>.ReferenceIn)), false, "[Required] A stream whose timestamps will be used as a reference for injecting default values."),
            new (typeof(DefaultValueInjector<>).GetProperty(nameof(DefaultValueInjector<short>.In)), true, "[Required] The input stream."),
            new (typeof(DefaultValueInjector<>).GetProperty(nameof(DefaultValueInjector<short>.Out)), false, "The output stream."),
        };

        public ComponentConfiguration CreateConfiguration() => new DefaultValueInjectorConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            Debug.Assert(Equals(this.OutputPorts().Single().Identifier, portConfiguration.Identifier));
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(instance));
            var result = HelperExtensions.CastProducerResult<T>(instance);
            return result;
        }
    }
}
