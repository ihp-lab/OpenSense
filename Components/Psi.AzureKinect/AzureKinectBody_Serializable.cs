#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Psi.AzureKinect;
using Newtonsoft.Json;

namespace OpenSense.Components.Psi.AzureKinect {

    using TFloat = Single;//Reduced precision from double to float, to reduce JSON file size.

    /// <summary>
    /// The default AzureKinectBody can not be serialized to JSON with default converter configurations.
    /// We do not have control of those configurations, so we create ourselves.
    /// </summary>
    public struct AzureKinectBody_Serializable {

        private static readonly PropertyInfo TrackingIdProperty = typeof(AzureKinectBody).GetProperty(nameof(AzureKinectBody.TrackingId));

        public readonly uint TrackingId;

        public readonly Dictionary<JointId, JointValueTuple> Joints = new Dictionary<JointId, JointValueTuple>(64);

        [JsonConstructor]
        private AzureKinectBody_Serializable(uint trackingId, Dictionary<JointId, JointValueTuple> joints) {
            TrackingId = trackingId;
            Joints = joints;
        }

        public AzureKinectBody_Serializable(AzureKinectBody body) {
            TrackingId = body.TrackingId;
            foreach (var item in body.Joints) {
                var val = new JointValueTuple(item.Value);
                Joints.Add(item.Key, val);
            }
        }

        public static explicit operator AzureKinectBody_Serializable(AzureKinectBody obj) => new AzureKinectBody_Serializable(obj);

        public static explicit operator AzureKinectBody(AzureKinectBody_Serializable obj) {
            var result = new AzureKinectBody();
            TrackingIdProperty.SetValue(result, obj.TrackingId);
            result.Joints.Clear();
            foreach (var joint in obj.Joints) {
                result.Joints[joint.Key] = (ValueTuple<CoordinateSystem, JointConfidenceLevel>)joint.Value;
            }
            return result;
        }

        public struct JointValueTuple {

            public readonly SerializableCoordinateSystem Pose;

            public readonly JointConfidenceLevel Confidence;

            [JsonConstructor]
            private JointValueTuple(SerializableCoordinateSystem pose, JointConfidenceLevel confidence) {
                Pose = pose;
                Confidence = confidence;
            }

            public JointValueTuple(ValueTuple<CoordinateSystem, JointConfidenceLevel> joint) {
                Confidence = joint.Item2;
                Pose = new SerializableCoordinateSystem(joint.Item1);
            }

            public static explicit operator JointValueTuple(ValueTuple<CoordinateSystem, JointConfidenceLevel> t) => new JointValueTuple(t);

            public static explicit operator ValueTuple<CoordinateSystem, JointConfidenceLevel>(JointValueTuple t) => ((CoordinateSystem)t.Pose, t.Confidence);


            public struct SerializableCoordinateSystem {

                public readonly Vector3 Origin;
                public readonly Vector3 XAxis;
                public readonly Vector3 YAxis;
                public readonly Vector3 ZAxis;

                [JsonConstructor]
                private SerializableCoordinateSystem(Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis) {
                    Origin = origin;
                    XAxis = xAxis;
                    YAxis = yAxis;
                    ZAxis = zAxis;
                }

                public SerializableCoordinateSystem(CoordinateSystem coordinateSystem) {
                    Origin = (Vector3)coordinateSystem.Origin;
                    XAxis = (Vector3)coordinateSystem.XAxis;
                    YAxis = (Vector3)coordinateSystem.YAxis;
                    ZAxis = (Vector3)coordinateSystem.ZAxis;
                }

                public static explicit operator SerializableCoordinateSystem(CoordinateSystem obj) => new SerializableCoordinateSystem(obj);

                public static explicit operator CoordinateSystem(SerializableCoordinateSystem obj) => new CoordinateSystem((Point3D)obj.Origin, (Vector3D)obj.XAxis, (Vector3D)obj.YAxis, (Vector3D)obj.ZAxis);

                public struct Vector3 {
                    
                    public readonly TFloat X;
                    public readonly TFloat Y;
                    public readonly TFloat Z;

                    [JsonConstructor]
                    private Vector3(TFloat x, TFloat y, TFloat z) {
                        X = x;
                        Y = y;
                        Z = z;
                    }

                    public Vector3(Point3D p) {
                        X = (TFloat)p.X;
                        Y = (TFloat)p.Y;
                        Z = (TFloat)p.Z;
                    }

                    public Vector3(Vector3D v) {
                        X = (TFloat)v.X;
                        Y = (TFloat)v.Y;
                        Z = (TFloat)v.Z;
                    }

                    public static explicit operator Vector3(Point3D p) => new Vector3(p);

                    public static explicit operator Vector3(Vector3D v) => new Vector3(v);

                    public static explicit operator Point3D(Vector3 p) => new Point3D(p.X, p.Y, p.Z);

                    public static explicit operator Vector3D(Vector3 v) => new Vector3D(v.X, v.Y, v.Z);
                }
            }
        }
    }
}
