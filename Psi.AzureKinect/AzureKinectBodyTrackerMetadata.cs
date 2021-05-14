using System;
using System.Composition;
using Microsoft.Psi.AzureKinect;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.AzureKinect {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectBodyTrackerMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that performs body tracking from the depth/IR images captured by the Azure Kinect sensor.";

        protected override Type ComponentType => typeof(AzureKinectBodyTracker);

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectBodyTrackerConfiguration();
    }
}
