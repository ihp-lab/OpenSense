#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public sealed class DeduplicatorConfiguration : GenericComponentConfiguration_OneParam {

        private static readonly DeduplicatorMetadata Metadata = new DeduplicatorMetadata();

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate<T>(Pipeline pipeline, IServiceProvider? serviceProvider) => new Deduplicator<T>(pipeline) {
        };
    }
}
