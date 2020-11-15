using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class FlipColorVideoMetadata : ConventionalComponentMetadata {

        public override string Description => "Flip color images.";

        protected override Type ComponentType => typeof(FlipColorVideo);

        public override ComponentConfiguration CreateConfiguration() => new FlipColorVideoConfiguration();
    }
}
