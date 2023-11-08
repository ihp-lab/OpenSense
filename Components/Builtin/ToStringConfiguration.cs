#nullable enable

using System;
using Microsoft.Psi;

namespace OpenSense.Components.Builtin {
    [Serializable]
    public sealed class ToStringConfiguration : ConventionalComponentConfiguration {

        private static readonly ToStringMetadata Metadata = new ToStringMetadata();

        public override IComponentMetadata GetMetadata() => Metadata;

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ToString(pipeline) { 
        };
    }
}
