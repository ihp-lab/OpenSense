using System;
using System.Composition;

namespace OpenSense.Components.OpenPose {
    [Export(typeof(IComponentMetadata))]
    public class OpenPoseMetadata : ConventionalComponentMetadata {

        public override string Description => "OpenPose by CMU Perceptual Computing Lab for image based body tracking. This wrapper of OpenPose requires Nvidia CUDA and limits up to 1 instance each process.";

        protected override Type ComponentType => typeof(OpenPose);

        public override string Name => "OpenPose";

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenPose.In):
                    return "[Required] images.";
                case nameof(OpenPose.Out):
                    return "[Composite] Tracking results.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenPoseConfiguration();
    }
}
