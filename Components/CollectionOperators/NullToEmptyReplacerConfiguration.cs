using System;
using Microsoft.Psi;

namespace OpenSense.Components.CollectionOperators {
    [Serializable]
    public sealed class NullToEmptyReplacerConfiguration : CollectionOperatorConfiguration {

        public override IComponentMetadata GetMetadata() => new NullToEmptyReplacerMetadata();

        protected override object Instantiate<TElem, TCollection>(Pipeline pipeline, IServiceProvider serviceProvider) =>
            new NullToEmptyReplacer<TElem, TCollection>(pipeline) { };
    }
}
