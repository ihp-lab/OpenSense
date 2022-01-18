using System;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenSense.Component.Head.Common;

namespace OpenSense.Component.EyePointOfInterest.Common {

    [Serializable]
    public class GazeToDisplayCoordinateMappingRecord : IEquatable<GazeToDisplayCoordinateMappingRecord> {
 
        public readonly Gaze Gaze;

        public readonly Pose HeadPose;

        /// <summary>
        /// 0 - 1 relative coordinate
        /// </summary>
        public readonly Vector2 Display;

        [JsonConstructor]
        public GazeToDisplayCoordinateMappingRecord(Gaze gaze, Pose headPose, Vector2 display) {
            Gaze = gaze;
            HeadPose = headPose;
            Display = display;
        }

        public GazeToDisplayCoordinateMappingRecord(PoseAndGaze headPoseAndGaze, Vector2 display) : this(headPoseAndGaze.Gaze, headPoseAndGaze.HeadPose, display) {}

        [JsonIgnore]
        public PoseAndGaze HeadPoseAndGaze => new PoseAndGaze(HeadPose, Gaze);

        public bool Equals(GazeToDisplayCoordinateMappingRecord other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Display.Equals(other.Display);
        }
    }
}
