using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.AzureKinect.Visualizer {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectBodyTrackerVisualizerMetadata : ConventionalComponentMetadata {

        public override string Description => "Visualize Azure Kinect body tracker's output.";

        protected override Type ComponentType => typeof(AzureKinectBodyTrackerVisualizer);

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerVisualizerConfiguration();
    }
}
