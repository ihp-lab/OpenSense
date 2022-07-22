using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Component.BodyGestureDetectors {
    public sealed class BodyLeaningDetector : IProducer<float>, INotifyPropertyChanged {

        #region Ports
        public Receiver<ImuSample> ImuIn { get; }

        public Receiver<List<AzureKinectBody>> BodiesIn { get; }

        public Emitter<float> Out { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }
        #endregion

        private ImuSample lastImu;

        public BodyLeaningDetector(Pipeline pipeline) {
            ImuIn = pipeline.CreateReceiver<ImuSample>(this, ProcessImu, nameof(ImuIn));
            BodiesIn = pipeline.CreateReceiver<List<AzureKinectBody>>(this, ProcessBodies, nameof(BodiesIn));
            Out = pipeline.CreateEmitter<float>(this, nameof(Out));
        }

        private void ProcessImu(ImuSample data, Envelope envelope) {
            if (data is null) {
                return;
            }
            if (lastImu is null) {
                lastImu = data.DeepClone();
            } else {
                data.DeepClone(ref lastImu);
            }
        }

        private void ProcessBodies(List<AzureKinectBody> bodies, Envelope envelope) {
            if (lastImu is null) {
                return;
            }
            if (bodies is null || bodies.Count <= bodyIndex) {
                return;
            }
            var body = bodies[bodyIndex];
            var (pelvis, pelvisConfidence) = body.Joints[JointId.Pelvis];
            var (neck, neckConfidence) = body.Joints[JointId.Neck];
            var (leftClavicle, leftClavicleConfidence) = body.Joints[JointId.ClavicleLeft];
            var (rightClavicle, rightClavicleConfidence) = body.Joints[JointId.ClavicleRight];
            if (new[] { pelvisConfidence, neckConfidence, leftClavicleConfidence, rightClavicleConfidence}.Any(confidence => (int)confidence < (int)JointConfidenceLevel.Medium)) {
                return;
            }
            //TODO
            //var groundTruthUpVector = 
            //var bodyUpVector = 

        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
