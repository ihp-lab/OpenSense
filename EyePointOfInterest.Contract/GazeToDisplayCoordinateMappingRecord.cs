using System;
using System.Numerics;
using Newtonsoft.Json;
using OpenSense.Component.Head.Common;

namespace OpenSense.Component.EyePointOfInterest.Common {

    [Serializable]
    public class GazeToDisplayCoordinateMappingRecord : IEquatable<GazeToDisplayCoordinateMappingRecord> {
 
        public readonly Gaze Gaze;

        public readonly HeadPose HeadPose;

        /// <summary>
        /// 0 - 1 relative coordinate
        /// </summary>
        public readonly Vector2 Display;

        [JsonConstructor]
        public GazeToDisplayCoordinateMappingRecord(Gaze gaze, HeadPose headPose, Vector2 display) {
            Gaze = gaze;
            HeadPose = headPose;
            Display = display;
        }

        public GazeToDisplayCoordinateMappingRecord(HeadPoseAndGaze headPoseAndGaze, Vector2 display) : this(headPoseAndGaze.Gaze, headPoseAndGaze.HeadPose, display) {}

        [JsonIgnore]
        public HeadPoseAndGaze HeadPoseAndGaze => new HeadPoseAndGaze(HeadPose, Gaze);

        public bool Equals(GazeToDisplayCoordinateMappingRecord other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Display.Equals(other.Display);
        }
    }
}
