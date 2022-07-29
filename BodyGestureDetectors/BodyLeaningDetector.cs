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
    public sealed class BodyLeaningDetector : INotifyPropertyChanged {

        #region Ports
        public Receiver<ImuSample> ImuIn { get; }

        public Receiver<List<AzureKinectBody>> BodiesIn { get; }

        public Emitter<float> RadianOut { get; }

        public Emitter<float> DegreeOut { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private float radianOffset = 0;

        public float RadianOffset {
            get => radianOffset;
            set => SetProperty(ref radianOffset, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }
        #endregion

        private ImuSample lastImu;

        public BodyLeaningDetector(Pipeline pipeline) {
            ImuIn = pipeline.CreateReceiver<ImuSample>(this, ProcessImu, nameof(ImuIn));
            BodiesIn = pipeline.CreateReceiver<List<AzureKinectBody>>(this, ProcessBodies, nameof(BodiesIn));
            RadianOut = pipeline.CreateEmitter<float>(this, nameof(RadianOut));
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
            var (spineChest, spineChestConfidence) = body.Joints[JointId.Pelvis];
            var (neck, neckConfidence) = body.Joints[JointId.Neck];
            var (leftClavicle, leftClavicleConfidence) = body.Joints[JointId.ClavicleLeft];
            var (rightClavicle, rightClavicleConfidence) = body.Joints[JointId.ClavicleRight];
            if (new[] { spineChestConfidence, neckConfidence, leftClavicleConfidence, rightClavicleConfidence}.Any(confidence => (int)confidence < (int)MinimumConfidenceLevel)) {
                return;
            }
            var up = lastImu.AccelerometerSample;
            var bodyUpRaw = neck.Origin - spineChest.Origin;
            var bodyUp = new Vector3(-(float)bodyUpRaw.X, -(float)bodyUpRaw.Y, -(float)bodyUpRaw.Z);//Actural returned coordinate is the opposite as Gyro coordinate and measured in meters, however, the doc says it is depth camera coordinate and measured in millimeters.
            var bodyRightRaw = rightClavicle.Origin - leftClavicle.Origin;
            var bodyRight = Vector3.Normalize(new Vector3((float)bodyRightRaw.X, (float)bodyRightRaw.Y, (float)bodyRightRaw.Z));//Values here then become the same direction as Gyro coordinate
            var radian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, bodyRight, bodyUp).Normal, up) - (float)Math.PI / 2f;
            radian += RadianOffset;
            RadianOut.Post(radian, envelope.OriginatingTime);
            var angle = 180f * (radian / (float)Math.PI);
            DegreeOut.Post(angle, envelope.OriginatingTime);
            ;
        }

        private static float RadianBetweenVectors(Vector3 v1, Vector3 v2) {
            var result = (float)Math.Acos(Vector3.Dot(v1, v2) / (v1.Length() * v2.Length()));
            return result;
        }

        private static Vector3 ConvertAccelerometerCoordinateToDepthCoordinate(Vector3 accCoordinate) {
            var rgbCoordinate = new Vector3(-accCoordinate.Y, accCoordinate.Z, -accCoordinate.X);
            var tiltRadian = Math.PI * (6f / 180);
            var depthCoordiante = Vector3.Transform(rgbCoordinate, Matrix4x4.CreateRotationX((float)tiltRadian));
            return depthCoordiante;
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
