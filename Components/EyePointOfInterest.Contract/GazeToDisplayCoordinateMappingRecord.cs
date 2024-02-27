using System;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenSense.Components.OpenFace;

namespace OpenSense.Components.EyePointOfInterest {
    [Serializable]
    public sealed class GazeToDisplayCoordinateMappingRecord : IEquatable<GazeToDisplayCoordinateMappingRecord> {
 
        public Eye Gaze { get; }

        public Pose HeadPose { get; }

        public Face Face { get; }

        /// <summary>
        /// 0 - 1 relative coordinate
        /// </summary>
        public Vector2 Display { get; }

        [JsonIgnore]
        public PoseAndEyeAndFace HeadPoseAndGaze => new PoseAndEyeAndFace(HeadPose, Gaze, Face);

        [JsonConstructor]
        public GazeToDisplayCoordinateMappingRecord(Eye gaze, Pose headPose, Face face, Vector2 display) {
            Gaze = gaze;
            HeadPose = headPose;
            Face = face;
            Display = display;
        }

        public GazeToDisplayCoordinateMappingRecord(PoseAndEyeAndFace headPoseAndGaze, Vector2 display) : this(headPoseAndGaze.Eye, headPoseAndGaze.Pose, headPoseAndGaze.Face, display) {
        }


        #region IEquatable
        public bool Equals(GazeToDisplayCoordinateMappingRecord other) =>
            Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Face.Equals(other.Face) && Display.Equals(other.Display);

        public override bool Equals(object obj) =>
            obj is GazeToDisplayCoordinateMappingRecord other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Gaze, HeadPose, Face, Display);

        public static bool operator ==(GazeToDisplayCoordinateMappingRecord left, GazeToDisplayCoordinateMappingRecord right) =>
            left.Equals(right);

        public static bool operator !=(GazeToDisplayCoordinateMappingRecord left, GazeToDisplayCoordinateMappingRecord right) =>
            !left.Equals(right);
        #endregion
    }
}
