using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [JsonObject]
    public class HeadPose : IEnumerable<double>, IEquatable<HeadPose> {

        /// <summary>
        /// Absolute head postion to camera in millimeter
        /// </summary>
        public readonly Point3D Position;

        /// <summary>
        /// Absolute head rotation to camera in radian
        /// </summary>
        public readonly Point3D Angle;

        public readonly ImmutableArray<Point2D> Landmarks;

        public readonly ImmutableArray<Point2D> VisiableLandmarks;

        public readonly ImmutableArray<Point3D> Landmarks3D;

        public readonly ImmutableArray<Tuple<Point2D, Point2D>> IndicatorLines;

        [JsonConstructor]
        public HeadPose(
            Point3D position,
            Point3D angle,
            ImmutableArray<Point2D> landmarks,
            ImmutableArray<Point2D> visiableLandmarks,
            ImmutableArray<Point3D> landmarks3D,
            ImmutableArray<Tuple<Point2D, Point2D>> indicatorLines
            ) {
            IndicatorLines = indicatorLines;
            Landmarks = landmarks;
            VisiableLandmarks = visiableLandmarks;
            Landmarks3D = landmarks3D;
            Position = position;
            Angle = angle;
        }

        public HeadPose(
            IList<float> data, 
            IEnumerable<Point2D> landmarks,
            IEnumerable<Point2D> visiableLandmarks,
            IEnumerable<Point3D> landmarks3D,
            IEnumerable<Tuple<Point2D, Point2D>> indicatorLines
            ) : this(
                new Point3D(data[0], data[1], data[2]), 
                new Point3D(data[3], data[4], data[5]), 
                landmarks.ToImmutableArray(), 
                visiableLandmarks.ToImmutableArray(), 
                landmarks3D.ToImmutableArray(),
                indicatorLines.ToImmutableArray()
                ) {}

        [JsonIgnore]
        public Point3D NoseTip3D => Landmarks3D[30];

        #region To accommodate old code
        public double this[int index] {
            get {
                switch (index) {
                    case 0:
                        return Position.X;
                    case 1:
                        return Position.Y;
                    case 2:
                        return Position.Z;
                    case 3:
                        return Angle.X;
                    case 4:
                        return Angle.Y;
                    case 5:
                        return Angle.Z;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        public int Count => 6;

        public IEnumerator<double> GetEnumerator() {
            for (var i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        public bool Equals(HeadPose other) {
            return Landmarks.SequenceEqual(other.Landmarks)
                && VisiableLandmarks.SequenceEqual(other.VisiableLandmarks)
                && Landmarks3D.SequenceEqual(other.Landmarks3D)
                && Position.Equals(other.Position)
                && Angle.Equals(other.Angle);
        }
    }
}
