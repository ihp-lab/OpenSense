using System;
using System.Composition;
using Microsoft.Psi.AzureKinect;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.AzureKinect {
    [Export(typeof(IComponentMetadata))]
    public class AzureKinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that captures all sensor streams and tracked bodies from the Azure Kinect device.";

        protected override Type ComponentType => typeof(AzureKinectSensor);

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectSensorConfiguration();
    }
}
