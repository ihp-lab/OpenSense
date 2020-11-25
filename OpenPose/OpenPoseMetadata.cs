using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenPose {
    [Export(typeof(IComponentMetadata))]
    public class OpenPoseMetadata : ConventionalComponentMetadata {

        public override string Description => "OpenPose by CMU-Perceptual-Computing-Lab (up to 1 instance each process).";

        protected override Type ComponentType => typeof(OpenPose);

        public override ComponentConfiguration CreateConfiguration() => new OpenPoseConfiguration();
    }
}
