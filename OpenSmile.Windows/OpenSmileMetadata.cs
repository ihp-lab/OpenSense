using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenSmile {
    [Export(typeof(IComponentMetadata))]
    public class OpenSmileMetadata : ConventionalComponentMetadata {

        public override string Description => "openSMILE by audEERING GmbH.";

        protected override Type ComponentType => typeof(OpenSmile);

        public override ComponentConfiguration CreateConfiguration() => new OpenSmileConfiguration();
    }
}
