using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenPose.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class OpenPoseVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize OpenPose results.";

        protected override Type ComponentType => typeof(OpenPoseVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new OpenPoseVisualizerConfiguration();
    }
}
