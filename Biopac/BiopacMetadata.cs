using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Biopac {
    [Export(typeof(IComponentMetadata))]
    public class BiopacMetadata : ConventionalComponentMetadata {

        public override string Description => "Biopac producer.";

        protected override Type ComponentType => typeof(Biopac);

        public override ComponentConfiguration CreateConfiguration() => new BiopacConfiguration();
    }
}
