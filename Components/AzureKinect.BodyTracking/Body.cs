using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.BodyTracking;
using KinectBody = Microsoft.Azure.Kinect.BodyTracking.Body;

namespace OpenSense.Components.AzureKinect.BodyTracking {
    /// <summary>
    /// Represents a body detected by the Azure Kinect.
    /// </summary>
    /// <remarks>This class replicates Microsoft.Psi.AzureKinect.AzureKinectBody.</remarks>
    public sealed class Body {

        private static readonly CoordinateSystem KinectBasis = new (default, UnitVector3D.ZAxis, UnitVector3D.XAxis.Negate(), UnitVector3D.YAxis.Negate());

        private static readonly CoordinateSystem KinectBasisInverted = KinectBasis.Invert();

        /// <summary>
        /// Gets the bone relationships.
        /// </summary>
        /// <remarks>
        /// Bone connections defined here: https://docs.microsoft.com/en-us/azure/Kinect-dk/body-joints.
        /// </remarks>
        public static IReadOnlyList<(JointId ChildJoint, JointId ParentJoint)> Bones { get; } = new List<(JointId, JointId)> {
            // Spine
            (JointId.SpineNavel, JointId.Pelvis),
            (JointId.SpineChest, JointId.SpineNavel),
            (JointId.Neck, JointId.SpineChest),

            // Left arm
            (JointId.ClavicleLeft, JointId.SpineChest),
            (JointId.ShoulderLeft, JointId.ClavicleLeft),
            (JointId.ElbowLeft, JointId.ShoulderLeft),
            (JointId.WristLeft, JointId.ElbowLeft),
            (JointId.HandLeft, JointId.WristLeft),
            (JointId.HandTipLeft, JointId.HandLeft),
            (JointId.ThumbLeft, JointId.WristLeft),

            // Right arm
            (JointId.ClavicleRight, JointId.SpineChest),
            (JointId.ShoulderRight, JointId.ClavicleRight),
            (JointId.ElbowRight, JointId.ShoulderRight),
            (JointId.WristRight, JointId.ElbowRight),
            (JointId.HandRight, JointId.WristRight),
            (JointId.HandTipRight, JointId.HandRight),
            (JointId.ThumbRight, JointId.WristRight),

            // Left leg
            (JointId.HipLeft, JointId.Pelvis),
            (JointId.KneeLeft, JointId.HipLeft),
            (JointId.AnkleLeft, JointId.KneeLeft),
            (JointId.FootLeft, JointId.AnkleLeft),

            // Right leg
            (JointId.HipRight, JointId.Pelvis),
            (JointId.KneeRight, JointId.HipRight),
            (JointId.AnkleRight, JointId.KneeRight),
            (JointId.FootRight, JointId.AnkleRight),

            // Head
            (JointId.Head, JointId.Neck),
            (JointId.Nose, JointId.Head),
            (JointId.EyeLeft, JointId.Head),
            (JointId.EarLeft, JointId.Head),
            (JointId.EyeRight, JointId.Head),
            (JointId.EarRight, JointId.Head),
        };

        /// <summary>
        /// Gets the body's tracking ID.
        /// </summary>
        public uint TrackingId { get; }

        /// <summary>
        /// Gets the joint information.
        /// </summary>
        public IReadOnlyDictionary<JointId, (CoordinateSystem Pose, JointConfidenceLevel Confidence)> Joints { get; }

        public Body(uint trackingId, IReadOnlyDictionary<JointId, (CoordinateSystem Pose, JointConfidenceLevel Confidence)> joints) {
            TrackingId = trackingId;
            Joints = joints.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        public Body(uint trackingId, KinectBody body) {
            TrackingId = trackingId;

            var joints = new Dictionary<JointId, (CoordinateSystem, JointConfidenceLevel)>();
            for (int i = 0; i < Skeleton.JointCount; i++) {
                var joint = body.Skeleton.GetJoint(i);
                var position = joint.Position;
                var orientation = joint.Quaternion;
                var confidence = joint.ConfidenceLevel;
                var val = (CreateCoordinateSystem(position, orientation), confidence);
                joints[(JointId)i] = val;
            }
            Joints = joints;
        }

        private static CoordinateSystem CreateCoordinateSystem(Vector3 position, System.Numerics.Quaternion orientation) {
            // Convert the quaternion to a rotation matrix (System.Numerics)
            var jointRotation = Matrix4x4.CreateFromQuaternion(orientation);

            // In the System.Numerics.Matrix4x4 joint rotation above, the axes of rotation are defined as follows:
            // X: [M11; M12; M13]
            // Y: [M21; M22; M23]
            // Z: [M31; M32; M33]
            // However, joint rotation axes are defined differently from the Azure Kinect sensor axes, as defined here:
            // https://docs.microsoft.com/en-us/azure/Kinect-dk/body-joints
            // and here:
            // https://docs.microsoft.com/en-us/azure/Kinect-dk/coordinate-systems
            // Joint Axes:
            //        X
            //        |   Y
            //        |  /
            //        | /
            // Z <----+
            // Azure Kinect Axes:
            //           Z
            //          /
            //         /
            //        +---->X
            //        |
            //        |
            //        |
            //        Y
            // Therefore we first create a transformation matrix in Azure Kinect basis by converting axes:
            // X (Azure Kinect) = -Z (Joint)
            // Y (Azure Kinect) = -X (Joint)
            // Z (Azure Kinect) =  Y (Joint)
            // and converting from millimeters to meters.
            var transformationMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { -jointRotation.M31, -jointRotation.M11, jointRotation.M21, position.X / 1000.0 },
                { -jointRotation.M32, -jointRotation.M12, jointRotation.M22, position.Y / 1000.0 },
                { -jointRotation.M33, -jointRotation.M13, jointRotation.M23, position.Z / 1000.0 },
                { 0,                  0,                  0,                 1 },
            });

            // Finally, convert from Azure Kinect's basis to MathNet's basis:
            return new CoordinateSystem(KinectBasisInverted * transformationMatrix * KinectBasis);
        }

        /// <inheritdoc/>
        public override string ToString() {
            var result = $"ID: {TrackingId}";
            return result;
        }
    }
}
