using System;
using System.Numerics;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Component.BodyGestureDetectors {
    public sealed class BodySwirlDetector : BodyJointBasedDetector {

        #region Ports
        #endregion

        #region Settings
        #endregion

        public BodySwirlDetector(Pipeline pipeline) : base(pipeline) {
        }

        protected override bool TryProcessJoints(ImuSample imuSample, AzureKinectBody body, out float radian) {
            var (spineChest, spineChestConfidence) = body.Joints[JointId.Pelvis];
            var (neck, neckConfidence) = body.Joints[JointId.Neck];
            var (leftClavicle, leftClavicleConfidence) = body.Joints[JointId.ClavicleLeft];
            var (rightClavicle, rightClavicleConfidence) = body.Joints[JointId.ClavicleRight];
            if (!ValidateJointConfidenceLevels(spineChestConfidence, neckConfidence, leftClavicleConfidence, rightClavicleConfidence)) {
                radian = float.NaN;
                return false;
            }
            var up = imuSample.AccelerometerSample;
            var outward = Vector3.Transform(up, Matrix4x4.CreateRotationY((float)Math.PI / 2f));
            var bodyUpRaw = neck.Origin - spineChest.Origin;
            var bodyUp = new Vector3(-(float)bodyUpRaw.X, -(float)bodyUpRaw.Y, -(float)bodyUpRaw.Z);//Actural returned coordinate is the opposite as Gyro coordinate and measured in meters, however, the doc says it is depth camera coordinate and measured in millimeters.
            var bodyRightRaw = rightClavicle.Origin - leftClavicle.Origin;
            var bodyRight = new Vector3((float)bodyRightRaw.X, (float)bodyRightRaw.Y, (float)bodyRightRaw.Z);//Values here then become the same direction as Gyro coordinate
            var bodyForward = Vector3.Cross(bodyUp, bodyRight);
            radian = RadianBetweenVectors(Plane.CreateFromVertices(Vector3.Zero, bodyForward, bodyUp).Normal, outward) - (float)Math.PI / 2;
            return true;
        }
    }
}
