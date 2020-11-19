using System;
using System.Composition;
using Microsoft.Psi.Kinect;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Kinect {
    [Export(typeof(IComponentMetadata))]
    public class KinectSensorMetadata : ConventionalComponentMetadata {

        public override string Description => "Component that captures and streams information (images, depth, audio, bodies, etc.) from a Kinect One (v2) sensor.";

        protected override Type ComponentType => typeof(KinectSensor);

        public override ComponentConfiguration CreateConfiguration() => new KinectSensorConfiguration();
    }
}
