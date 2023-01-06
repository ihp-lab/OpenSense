using System;
using Microsoft.Psi;
using Microsoft.Psi.Kinect;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Psi.Kinect {
    [Serializable]
    public class KinectSensorConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.Kinect.KinectSensorConfiguration raw = Microsoft.Psi.Kinect.KinectSensorConfiguration.Default;

        public Microsoft.Psi.Kinect.KinectSensorConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new KinectSensorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new KinectSensor(pipeline, Raw);
    }
}
