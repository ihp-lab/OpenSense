using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [JsonObject]
    public class Pose : IEnumerable<double>, IEquatable<Pose> {

        /// <summary>
        /// Absolute head postion to camera in millimeter
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// Absolute head rotation to camera in radian
        /// </summary>
        public readonly Vector3 Angle;

        public readonly ImmutableArray<Vector2> Landmarks;

        public readonly ImmutableArray<Vector2> VisiableLandmarks;

        public readonly ImmutableArray<Vector3> Landmarks3D;

        public readonly ImmutableArray<ValueTuple<Vector2, Vector2>> IndicatorLines;

        [JsonConstructor]
        public Pose(
            Vector3 position,
            Vector3 angle,
            ImmutableArray<Vector2> landmarks,
            ImmutableArray<Vector2> visiableLandmarks,
            ImmutableArray<Vector3> landmarks3D,
            ImmutableArray<ValueTuple<Vector2, Vector2>> indicatorLines
            ) {
            IndicatorLines = indicatorLines;
            Landmarks = landmarks;
            VisiableLandmarks = visiableLandmarks;
            Landmarks3D = landmarks3D;
            Position = position;
            Angle = angle;
        }

        public Pose(
            IList<float> data, 
            IEnumerable<Vector2> landmarks,
            IEnumerable<Vector2> visiableLandmarks,
            IEnumerable<Vector3> landmarks3D,
            IEnumerable<ValueTuple<Vector2, Vector2>> indicatorLines
            ) : this(
                new Vector3(data[0], data[1], data[2]), 
                new Vector3(data[3], data[4], data[5]), 
                landmarks.ToImmutableArray(), 
                visiableLandmarks.ToImmutableArray(), 
                landmarks3D.ToImmutableArray(),
                indicatorLines.ToImmutableArray()
                ) {}

        [JsonIgnore]
        public Vector3 NoseTip3D => Landmarks3D[30];

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

        public bool Equals(Pose other) {
            return Landmarks.SequenceEqual(other.Landmarks)
                && VisiableLandmarks.SequenceEqual(other.VisiableLandmarks)
                && Landmarks3D.SequenceEqual(other.Landmarks3D)
                && Position.Equals(other.Position)
                && Angle.Equals(other.Angle);
        }
    }
}
