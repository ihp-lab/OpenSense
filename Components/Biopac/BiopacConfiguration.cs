using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Biopac {
    [Serializable]
    public class BiopacConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new BiopacMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new Biopac(pipeline);
    }
}
