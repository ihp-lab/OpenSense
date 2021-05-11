using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class Gaze : IEquatable<Gaze> {
        /// <summary>
        /// Normalized left pupil vector to camera
        /// </summary>
        public readonly Pupil GazeVector;

        /// <summary>
        /// Absolute gaze angle to camera in radian、
        /// mean of eyes
        /// </summary>
        public readonly Vector2 Angle;

        public readonly ImmutableArray<Vector2> Landmarks;

        public readonly ImmutableArray<Vector3> Landmarks3D;

        [JsonIgnore]
        public readonly ImmutableArray<Vector2> VisiableLandmarks;

        [JsonIgnore]
        public readonly ImmutableArray<ValueTuple<Vector2, Vector2>> IndicatorLines;

        [JsonConstructor]
        public Gaze(Pupil gazeVector, Vector2 angle, IEnumerable<Vector2> landmarks, IEnumerable<Vector3> landmarks3D) : this(gazeVector, angle, landmarks, Array.Empty<Vector2>(), landmarks3D, Array.Empty<ValueTuple<Vector2, Vector2>>()) { }

        public Gaze(
            Pupil gazeVector, 
            Vector2 angle,
            IEnumerable<Vector2> landmarks,
            IEnumerable<Vector2> visiableLandmarks,
            IEnumerable<Vector3> landmarks3D,
            IEnumerable<ValueTuple<Vector2, Vector2>> indicatorLines
            ) {
            IndicatorLines = indicatorLines.ToImmutableArray();
            Landmarks = landmarks.ToImmutableArray();
            VisiableLandmarks = visiableLandmarks.ToImmutableArray();
            Landmarks3D = landmarks3D.ToImmutableArray();
            GazeVector = gazeVector;
            Angle = angle;
        }

        [JsonIgnore]
        public Pupil PupilPosition {
            get {
                var leftLandmarks = Landmarks3D.Skip(0).Take(8).ToList();
                var leftSum = leftLandmarks.Aggregate((a, b) => a + b);
                var left = leftSum / leftLandmarks.Count;
                var rightLandmarks = Landmarks3D.Skip(28).Take(8).ToList();
                var rightSum = rightLandmarks.Aggregate((a, b) => a + b);
                var right = rightSum / rightLandmarks.Count;
                return new Pupil(left, right);
            }
        }

        [JsonIgnore]
        public Pupil InnerEyeCornerPosition => new Pupil(Landmarks3D[14], Landmarks3D[36]);

        public bool Equals(Gaze other) {
            return Landmarks.SequenceEqual(other.Landmarks)
                && VisiableLandmarks.SequenceEqual(other.VisiableLandmarks)
                && Landmarks3D.SequenceEqual(other.Landmarks3D)
                && GazeVector.Equals(other.GazeVector)
                && Angle.Equals(other.Angle);
        }
    }
}
