using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.CollectionOperators {
    [Serializable]
    public sealed class NullToEmptyReplacerConfiguration : CollectionOperatorConfiguration {

        public override IComponentMetadata GetMetadata() => new NullToEmptyReplacerMetadata();

        protected override object Instantiate<TElem, TCollection>(Pipeline pipeline, IServiceProvider serviceProvider) =>
            new NullToEmptyReplacer<TElem, TCollection>(pipeline) { };
    }
}
