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

namespace OpenSense.Components.BodyGestureDetectors {

    public sealed class BodyAttitudeDetector {
        #region Ports
        public Receiver<ImuSample> ImuIn { get; }

        public Receiver<List<AzureKinectBody>> BodiesIn { get; }

        public Emitter<float> YawRadianOut { get; }

        public Emitter<float> YawDegreeOut { get; }

        public Emitter<float> PitchRadianOut { get; }

        public Emitter<float> PitchDegreeOut { get; }

        public Emitter<float> RollRadianOut { get; }

        public Emitter<float> RollDegreeOut { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private float yawOffset = 0;

        public float YawOffset {
            get => yawOffset;
            set => SetProperty(ref yawOffset, value);
        }

        private float pitchOffset = 0;

        public float PitchOffset {
            get => pitchOffset;
            set => SetProperty(ref pitchOffset, value);
        }

        private float rollOffset = 0;

        public float RollOffset {
            get => rollOffset;
            set => SetProperty(ref rollOffset, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }
        #endregion

        private ImuSample lastImu;

        public BodyAttitudeDetector(Pipeline pipeline) {
            ImuIn = pipeline.CreateReceiver<ImuSample>(this, ProcessImu, nameof(ImuIn));
            BodiesIn = pipeline.CreateReceiver<List<AzureKinectBody>>(this, ProcessBodies, nameof(BodiesIn));
            YawRadianOut = pipeline.CreateEmitter<float>(this, nameof(YawRadianOut));
            YawDegreeOut = pipeline.CreateEmitter<float>(this, nameof(YawDegreeOut));
            PitchRadianOut = pipeline.CreateEmitter<float>(this, nameof(PitchRadianOut));
            PitchDegreeOut = pipeline.CreateEmitter<float>(this, nameof(PitchDegreeOut));
            RollRadianOut = pipeline.CreateEmitter<float>(this, nameof(RollRadianOut));
            RollDegreeOut = pipeline.CreateEmitter<float>(this, nameof(RollDegreeOut));
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
            if (!ValidateJointConfidenceLevels(spineChestConfidence, neckConfidence, leftClavicleConfidence, rightClavicleConfidence)) {
                return;
            }
            var up = lastImu.AccelerometerSample;
            var outward = Vector3.Transform(up, Matrix4x4.CreateRotationY((float)Math.PI / 2f));
            var right = Vector3.Transform(up, Matrix4x4.CreateRotationX((float)Math.PI / 2f));
            
            var bodyUpRaw = neck.Origin - spineChest.Origin;
            var bodyUp = new Vector3(-(float)bodyUpRaw.X, -(float)bodyUpRaw.Y, -(float)bodyUpRaw.Z);//Actural returned coordinate is the opposite as Gyro coordinate and measured in meters, however, the doc says it is depth camera coordinate and measured in millimeters.
            var bodyRightRaw = rightClavicle.Origin - leftClavicle.Origin;
            var bodyRight = new Vector3((float)bodyRightRaw.X, (float)bodyRightRaw.Y, (float)bodyRightRaw.Z);//Values here then become the same direction as Gyro coordinate
            var bodyForward = Vector3.Cross(bodyUp, bodyRight);

            var yawRadian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, bodyForward, bodyUp).Normal, outward) - (float)Math.PI / 2;
            yawRadian += YawOffset;
            YawRadianOut.Post(yawRadian, envelope.OriginatingTime);
            var yawDegree = 180f * (yawRadian / (float)Math.PI);
            YawDegreeOut.Post(yawDegree, envelope.OriginatingTime);

            var pitchRadian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, bodyRight, bodyUp).Normal, up) - (float)Math.PI / 2f;
            pitchRadian += PitchOffset;
            PitchRadianOut.Post(pitchRadian, envelope.OriginatingTime);
            var pitchDegree = 180f * (pitchRadian / (float)Math.PI);
            PitchDegreeOut.Post(pitchDegree, envelope.OriginatingTime);

            var rollRadian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, bodyRight, bodyForward).Normal, right) - (float)Math.PI / 2f;
            rollRadian += RollOffset;
            RollRadianOut.Post(rollRadian, envelope.OriginatingTime);
            var rollDegree = 180f * (rollRadian / (float)Math.PI);
            RollDegreeOut.Post(rollDegree, envelope.OriginatingTime);
        }

        #region Helpers
        private bool ValidateJointConfidenceLevels(params JointConfidenceLevel[] confidenceLevels) {
            var result = confidenceLevels.All(confidence => (int)confidence >= (int)MinimumConfidenceLevel);
            return result;
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
