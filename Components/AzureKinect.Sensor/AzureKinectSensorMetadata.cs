using System;
using System.Composition;

namespace OpenSense.Components.AzureKinect.Sensor {
    [Export(typeof(IComponentMetadata))]
    public sealed class AzureKinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description =>
            ""
            ;

        protected override Type ComponentType => typeof(AzureKinectSensor);

        public override string Name => "Azure Kinect Sensor";

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new AzureKinectSensorConfiguration();
    }
}
