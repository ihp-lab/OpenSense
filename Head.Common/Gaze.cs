using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MathNet.Spatial.Euclidean;
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
        public readonly Point2D Angle;

        public readonly ImmutableArray<Point2D> Landmarks;

        public readonly ImmutableArray<Point3D> Landmarks3D;

        [JsonIgnore]
        public readonly ImmutableArray<Point2D> VisiableLandmarks;

        [JsonIgnore]
        public readonly ImmutableArray<Tuple<Point2D, Point2D>> IndicatorLines;

        [JsonConstructor]
        public Gaze(Pupil gazeVector, Point2D angle, IEnumerable<Point2D> landmarks, IEnumerable<Point3D> landmarks3D) : this(gazeVector, angle, landmarks, Array.Empty<Point2D>(), landmarks3D, Array.Empty<Tuple<Point2D, Point2D>>()) { }

        public Gaze(
            Pupil gazeVector, 
            Point2D angle,
            IEnumerable<Point2D> landmarks,
            IEnumerable<Point2D> visiableLandmarks,
            IEnumerable<Point3D> landmarks3D,
            IEnumerable<Tuple<Point2D, Point2D>> indicatorLines
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
                var left = Point3D.Centroid(Landmarks3D.Skip(0).Take(8));
                var right = Point3D.Centroid(Landmarks3D.Skip(28).Take(8));
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
