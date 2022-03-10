using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class Eye : IEquatable<Eye> {
        /// <summary>
        /// Normalized left pupil vector to camera
        /// </summary>
        [JsonInclude]
        public readonly GazeVector GazeVector;

        /// <summary>
        /// Absolute gaze angle to camera in radian、
        /// mean of eyes
        /// </summary>
        [JsonInclude]
        public readonly Vector2 Angle;

        [JsonInclude]
        public readonly IReadOnlyList<Vector2> Landmarks;

        [JsonInclude]
        public readonly IReadOnlyList<Vector3> Landmarks3D;

        [JsonInclude]
        public readonly IReadOnlyList<Vector2> VisiableLandmarks;

        [JsonInclude]
        public readonly IReadOnlyList<ValueTuple<Vector2, Vector2>> IndicatorLines;

        [JsonConstructor]
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
            Landmarks3D = landmarks3D.ToImmutableArray();
            VisiableLandmarks = visiableLandmarks.ToImmutableArray();
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
