using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Component.BodyGestureDetectors {
    public abstract class BodyJointBasedDetector : IProducer<float> {
        #region Ports
        public Receiver<ImuSample> ImuIn { get; }

        public Receiver<List<AzureKinectBody>> BodiesIn { get; }

        public Emitter<float> Out { get; }

        public Emitter<float> DegreeOut { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private float outputOffset = 0;

        public float OutputOffset {
            get => outputOffset;
            set => SetProperty(ref outputOffset, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }
        #endregion

        private ImuSample lastImu;

        public BodyJointBasedDetector(Pipeline pipeline) {
            ImuIn = pipeline.CreateReceiver<ImuSample>(this, ProcessImu, nameof(ImuIn));
            BodiesIn = pipeline.CreateReceiver<List<AzureKinectBody>>(this, ProcessBodies, nameof(BodiesIn));
            Out = pipeline.CreateEmitter<float>(this, nameof(Out));
            DegreeOut = pipeline.CreateEmitter<float>(this, nameof(DegreeOut));
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
            if (TryProcessJoints(lastImu, body, out var radian)) {
                radian += OutputOffset;
                Out.Post(radian, envelope.OriginatingTime);
                var degree = 180f * (radian / (float)Math.PI);
                DegreeOut.Post(degree, envelope.OriginatingTime);
            }
        }

        #region APIs
        protected abstract bool TryProcessJoints(ImuSample imuSample, AzureKinectBody body, out float radian); 
        #endregion

        #region Helpers
        protected bool ValidateJointConfidenceLevels(params JointConfidenceLevel[] confidenceLevels) {
            var result = confidenceLevels.All(confidence => (int)confidence >= (int)MinimumConfidenceLevel);
            return result;
        }

        protected static float RadianBetweenVectors(Vector3 v1, Vector3 v2) {
            var result = (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length() * v2.Length()));
            return result;
        }

        protected static Vector3 ConvertAccelerometerCoordinateToDepthCoordinate(Vector3 accCoordinate) {
            var rgbCoordinate = new Vector3(-accCoordinate.Y, accCoordinate.Z, -accCoordinate.X);
            var tiltRadian = Math.PI * (6f / 180);
            var depthCoordiante = Vector3.Transform(rgbCoordinate, Matrix4x4.CreateRotationX((float)tiltRadian));
            return depthCoordiante;
        } 
        #endregion

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
