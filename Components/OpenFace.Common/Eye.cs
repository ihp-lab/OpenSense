using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    [Serializable]
    public sealed class Eye : IEquatable<Eye> {
        /// <summary>
        /// Normalized left pupil vector to camera
        /// </summary>
        public GazeVector GazeVector { get; }

        /// <summary>
        /// Absolute gaze angle to camera in radian、
        /// mean of eyes
        /// </summary>
        public Vector2 Angle { get; }

        public IReadOnlyList<Vector2> Landmarks { get; }

        public IReadOnlyList<Vector2> VisiableLandmarks { get; }

        public IReadOnlyList<Vector3> Landmarks3D { get; }

        public IReadOnlyList<ValueTuple<Vector2, Vector2>> IndicatorLines { get; }

        [JsonConstructor]
        internal Eye(
            GazeVector gazeVector, 
            Vector2 angle, 
            IReadOnlyList<Vector2> landmarks, 
            IReadOnlyList<Vector2> visiableLandmarks, 
            IReadOnlyList<Vector3> landmarks3D, 
            IReadOnlyList<(Vector2, Vector2)> indicatorLines
            ) {
            GazeVector = gazeVector;
            Angle = angle;
            Landmarks = landmarks;
            VisiableLandmarks = visiableLandmarks;
            Landmarks3D = landmarks3D;
            IndicatorLines = indicatorLines;
        }

        public Eye(
            GazeVector gazeVector, 
            Vector2 angle,
            IEnumerable<Vector2> landmarks,
            IEnumerable<Vector2> visiableLandmarks,
            IEnumerable<Vector3> landmarks3D,
            IEnumerable<ValueTuple<Vector2, Vector2>> indicatorLines
            ) {
            GazeVector = gazeVector;
            Angle = angle;
            Landmarks = landmarks.ToImmutableArray();
            VisiableLandmarks = visiableLandmarks.ToImmutableArray();
            Landmarks3D = landmarks3D.ToImmutableArray();
            IndicatorLines = indicatorLines.ToImmutableArray();
        }

        [JsonIgnore]
        public GazeVector PupilPosition {
            get {
                var leftLandmarks = Landmarks3D.Skip(0).Take(8).ToList();
                var leftSum = leftLandmarks.Aggregate((a, b) => a + b);
                var left = leftSum / leftLandmarks.Count;
                var rightLandmarks = Landmarks3D.Skip(28).Take(8).ToList();
                var rightSum = rightLandmarks.Aggregate((a, b) => a + b);
                var right = rightSum / rightLandmarks.Count;
                return new GazeVector(left, right);
            }
        }

        [JsonIgnore]
        public GazeVector InnerEyeCornerPosition => new GazeVector(Landmarks3D[14], Landmarks3D[36]);

        #region IEquatable
        public bool Equals(Eye other) =>
            Landmarks.SequenceEqual(other.Landmarks)
            && VisiableLandmarks.SequenceEqual(other.VisiableLandmarks)
            && Landmarks3D.SequenceEqual(other.Landmarks3D)
            && GazeVector.Equals(other.GazeVector)
            && Angle.Equals(other.Angle);

        public override bool Equals(object obj) => obj is Eye other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            Landmarks,
            VisiableLandmarks,
            Landmarks3D,
            GazeVector,
            Angle
        );

        public static bool operator ==(Eye a, Eye b) => a.Equals(b);

        public static bool operator !=(Eye a, Eye b) => !(a == b);
        #endregion
    }
}
