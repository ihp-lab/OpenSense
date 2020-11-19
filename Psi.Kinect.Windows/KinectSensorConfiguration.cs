using System;
using Microsoft.Psi;
using Microsoft.Psi.Kinect;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Kinect {
    [Serializable]
    public class KinectSensorConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Kinect.KinectSensorConfiguration raw = Microsoft.Psi.Kinect.KinectSensorConfiguration.Default;

        public Microsoft.Psi.Kinect.KinectSensorConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new KinectSensorMetadata();

        protected override object Instantiate(Pipeline pipeline) => new KinectSensor(pipeline, Raw);
    }
}
