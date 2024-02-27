using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    //[JsonObject]
    [Serializable]
    public sealed class Pose /*: IEnumerable<double>, IEquatable<Pose>*/ {//Interfaces removed since no support for JsonObjectAttribute after Json.Net is removed

        /// <summary>
        /// Absolute head postion to camera in millimeter
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Absolute head rotation to camera in radian
        /// </summary>
        public Vector3 Angle { get; }

        public IReadOnlyList<Vector2> Landmarks { get; }

        public IReadOnlyList<Vector2> VisiableLandmarks { get; }

        public IReadOnlyList<Vector3> Landmarks3D { get; }

        public IReadOnlyList<(Vector2, Vector2)> IndicatorLines { get; }

        [JsonConstructor]
        internal Pose(
            Vector3 position, 
            Vector3 angle, 
            IReadOnlyList<Vector2> landmarks, 
            IReadOnlyList<Vector2> visiableLandmarks, 
            IReadOnlyList<Vector3> landmarks3D, 
            IReadOnlyList<(Vector2, Vector2)> indicatorLines
            ) {
            Position = position;
            Angle = angle;
            Landmarks = landmarks;
            VisiableLandmarks = visiableLandmarks;
            Landmarks3D = landmarks3D;
            IndicatorLines = indicatorLines;
        }

        public Pose(
            Vector3 position,
            Vector3 angle,
            ImmutableArray<Vector2> landmarks,
            ImmutableArray<Vector2> visiableLandmarks,
            ImmutableArray<Vector3> landmarks3D,
            ImmutableArray<(Vector2, Vector2)> indicatorLines
            ) {
            Position = position;
            Angle = angle;
            Landmarks = landmarks;
            VisiableLandmarks = visiableLandmarks;
            Landmarks3D = landmarks3D;
            IndicatorLines = indicatorLines;
        }

        public Pose(
            IList<float> data, 
            IEnumerable<Vector2> landmarks,
            IEnumerable<Vector2> visiableLandmarks,
            IEnumerable<Vector3> landmarks3D,
            IEnumerable<(Vector2, Vector2)> indicatorLines
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
        [JsonIgnore]
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

        [JsonIgnore]
        public int Count => 6;

        public IEnumerator<double> GetEnumerator() {
            for (var i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        //IEnumerator IEnumerable.GetEnumerator() {
        //    return GetEnumerator();
        //}

        #endregion

        #region IEquatable
        public bool Equals(Pose other) => 
            Landmarks.SequenceEqual(other.Landmarks)
            && VisiableLandmarks.SequenceEqual(other.VisiableLandmarks)
            && Landmarks3D.SequenceEqual(other.Landmarks3D)
            && Position.Equals(other.Position)
            && Angle.Equals(other.Angle);

        public override bool Equals(object obj) => obj is Pose other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            Landmarks,
            VisiableLandmarks,
            Landmarks3D,
            Position,
            Angle
        );

        public static bool operator ==(Pose a, Pose b) => a.Equals(b);

        public static bool operator !=(Pose a, Pose b) => !(a == b);
        #endregion
    }
}
