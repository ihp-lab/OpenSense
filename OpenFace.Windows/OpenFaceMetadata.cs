using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenFace {
    [Export(typeof(IComponentMetadata))]
    public class OpenFaceMetadata : ConventionalComponentMetadata {

        public override string Description => "OpenFace by MultiComp-Lab.";

        protected override Type ComponentType => typeof(OpenFace);

        public override ComponentConfiguration CreateConfiguration() => new OpenFaceConfiguration();
    }
}
