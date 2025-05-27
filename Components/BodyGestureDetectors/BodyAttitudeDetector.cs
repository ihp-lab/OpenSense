using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Psi;
using Body = OpenSense.Components.AzureKinect.BodyTracking.Body;

namespace OpenSense.Components.BodyGestureDetectors {
    /// <remarks>
    /// Outputs are reletaive to a coordinate system that origin from Azure Kinect but with its Z-axis aligned to the gravity direction.
    /// </remarks>

    public sealed class BodyAttitudeDetector {
        #region Ports
        public Receiver<ImuSample> ImuIn { get; }

        public Receiver<Body[]?> BodiesIn { get; }

        /* Torso */
        public Emitter<float> YawRadianOut { get; }

        public Emitter<float> YawDegreeOut { get; }

        public Emitter<float> PitchRadianOut { get; }

        public Emitter<float> PitchDegreeOut { get; }

        public Emitter<float> RollRadianOut { get; }

        public Emitter<float> RollDegreeOut { get; }

        /* Head */
        public Emitter<float> HeadYawRadianOut { get; }

        public Emitter<float> HeadYawDegreeOut { get; }

        public Emitter<float> HeadPitchRadianOut { get; }

        public Emitter<float> HeadPitchDegreeOut { get; }

        public Emitter<float> HeadRollRadianOut { get; }

        public Emitter<float> HeadRollDegreeOut { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }


        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Medium;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        /* Torso */
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

        /* Head */
        private float headYawOffset = 0;

        public float HeadYawOffset {
            get => headYawOffset;
            set => SetProperty(ref headYawOffset, value);
        }

        private float headPitchOffset = 0;

        public float HeadPitchOffset {
            get => headPitchOffset;
            set => SetProperty(ref headPitchOffset, value);
        }

        private float headRollOffset = 0;

        public float HeadRollOffset {
            get => headRollOffset;
            set => SetProperty(ref headRollOffset, value);
        }
        #endregion

        private ImuSample? lastImu;

        public BodyAttitudeDetector(Pipeline pipeline) {
            ImuIn = pipeline.CreateReceiver<ImuSample>(this, ProcessImu, nameof(ImuIn));
            BodiesIn = pipeline.CreateReceiver<Body[]?>(this, ProcessBodies, nameof(BodiesIn));
            /* Torso */
            YawRadianOut = pipeline.CreateEmitter<float>(this, nameof(YawRadianOut));
            YawDegreeOut = pipeline.CreateEmitter<float>(this, nameof(YawDegreeOut));
            PitchRadianOut = pipeline.CreateEmitter<float>(this, nameof(PitchRadianOut));
            PitchDegreeOut = pipeline.CreateEmitter<float>(this, nameof(PitchDegreeOut));
            RollRadianOut = pipeline.CreateEmitter<float>(this, nameof(RollRadianOut));
            RollDegreeOut = pipeline.CreateEmitter<float>(this, nameof(RollDegreeOut));
            /* Head */
            HeadYawRadianOut = pipeline.CreateEmitter<float>(this, nameof(HeadYawRadianOut));
            HeadYawDegreeOut = pipeline.CreateEmitter<float>(this, nameof(HeadYawDegreeOut));
            HeadPitchRadianOut = pipeline.CreateEmitter<float>(this, nameof(HeadPitchRadianOut));
            HeadPitchDegreeOut = pipeline.CreateEmitter<float>(this, nameof(HeadPitchDegreeOut));
            HeadRollRadianOut = pipeline.CreateEmitter<float>(this, nameof(HeadRollRadianOut));
            HeadRollDegreeOut = pipeline.CreateEmitter<float>(this, nameof(HeadRollDegreeOut));
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

        private void ProcessBodies(Body[]? bodies, Envelope envelope) {
            if (lastImu is null) {
                return;
            }
            if (bodies is null || bodies.Length <= bodyIndex) {
                return;
            }
            var body = bodies[bodyIndex];

            /* Azure Kinect coordinate. Right hand. https://learn.microsoft.com/en-us/azure/kinect-dk/coordinate-systems */
            var up = Vector3.Normalize(lastImu.AccelerometerSample);//When laid flat, Z prop is close to -1.0
            var outward = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateRotationY((float)Math.PI / 2f)));//When laid flat, X prop is close to -1.0
            var right = Vector3.Normalize(Vector3.Transform(up, Matrix4x4.CreateRotationX((float)Math.PI / 2f)));//When laid flat, Y prop is close to 1.0

            /* Note:
             * About body joint coordinate, X axis is pointing back to the user, Y axis is pointing right to the user, Z axis is pointing up to the user, distance is measured in meters.
             * This is different from Azure Kinect's documentation. 
             * Here is the code that does the conversion, https://github.com/microsoft/psi/blob/cb2651f8e591c63d4a1fc8a16ad08ec7196338eb/Sources/Kinect/Microsoft.Psi.AzureKinect.x64/AzureKinectBody.cs#LL117C114-L117C114
             * That code might not be correct and it makes our computation become hard.
             * So, here as long as the final result looks correct, we are good.
             * Do not try to understand the computation.
             */

            /* Torso */
            var (spineChest, spineChestConfidence) = body.Joints[JointId.Pelvis];
            var (neck, neckConfidence) = body.Joints[JointId.Neck];
            var (leftClavicle, leftClavicleConfidence) = body.Joints[JointId.ClavicleLeft];
            var (rightClavicle, rightClavicleConfidence) = body.Joints[JointId.ClavicleRight];
            if (ValidateJointConfidenceLevels(spineChestConfidence, neckConfidence, leftClavicleConfidence, rightClavicleConfidence)) {
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

            /* Head */
            var (head, headConfidence) = body.Joints[JointId.Head];
            if (ValidateJointConfidenceLevels(headConfidence)) {
                var headUpRaw = head.ZAxis;//When sit facing to kinect, Z prop is close to 1.0
                var headUp = new Vector3((float)headUpRaw.X, (float)headUpRaw.Y, (float)headUpRaw.Z);
                var headRightRaw = head.YAxis;//When sit facing to kinect, Y prop is close to -1.0
                var headRight = new Vector3((float)headRightRaw.X, (float)headRightRaw.Y, (float)headRightRaw.Z);
                var headForward = Vector3.Cross(headUp, headRight);

                var yawRadian = -(RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, headForward, headUp).Normal, outward) - (float)Math.PI / 2);
                yawRadian += HeadYawOffset;
                HeadYawRadianOut.Post(yawRadian, envelope.OriginatingTime);
                var yawDegree = 180f * (yawRadian / (float)Math.PI);
                HeadYawDegreeOut.Post(yawDegree, envelope.OriginatingTime);

                var pitchRadian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, headRight, headUp).Normal, up) - (float)Math.PI / 2f;
                pitchRadian += HeadPitchOffset;
                HeadPitchRadianOut.Post(pitchRadian, envelope.OriginatingTime);
                var pitchDegree = 180f * (pitchRadian / (float)Math.PI);
                HeadPitchDegreeOut.Post(pitchDegree, envelope.OriginatingTime);

                var rollRadian = -(RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, headRight, headForward).Normal, right) - (float)Math.PI / 2f);
                rollRadian += HeadRollOffset;
                HeadRollRadianOut.Post(rollRadian, envelope.OriginatingTime);
                var rollDegree = 180f * (rollRadian / (float)Math.PI);
                HeadRollDegreeOut.Post(rollDegree, envelope.OriginatingTime);
            }
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
            var tiltRadian = Math.PI * (-6f / 180);
            var depthCoordiante = Vector3.Transform(rgbCoordinate, Matrix4x4.CreateRotationX((float)tiltRadian));
            return depthCoordiante;
        } 
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
