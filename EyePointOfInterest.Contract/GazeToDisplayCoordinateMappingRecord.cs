using System;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenSense.Component.OpenFace.Common;

namespace OpenSense.Component.EyePointOfInterest.Common {

    [Serializable]
    public class GazeToDisplayCoordinateMappingRecord : IEquatable<GazeToDisplayCoordinateMappingRecord> {
 
        public readonly Eye Gaze;

        public readonly Pose HeadPose;

        public readonly Face Face;

        /// <summary>
        /// 0 - 1 relative coordinate
        /// </summary>
        public readonly Vector2 Display;

        [JsonConstructor]
        public GazeToDisplayCoordinateMappingRecord(Eye gaze, Pose headPose, Face face, Vector2 display) {
            Gaze = gaze;
            HeadPose = headPose;
            Face = face;
            Display = display;
        }

        public GazeToDisplayCoordinateMappingRecord(PoseAndEyeAndFace headPoseAndGaze, Vector2 display) : this(headPoseAndGaze.Eye, headPoseAndGaze.Pose, headPoseAndGaze.Face, display) {}

        [JsonIgnore]
        public PoseAndEyeAndFace HeadPoseAndGaze => new PoseAndEyeAndFace(HeadPose, Gaze, Face);

        public bool Equals(GazeToDisplayCoordinateMappingRecord other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Face.Equals(other.Face) && Display.Equals(other.Display);
        }
    }
}
