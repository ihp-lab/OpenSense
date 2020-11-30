using System;
using MathNet.Spatial.Euclidean;
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
        public readonly Point2D Display;

        [JsonConstructor]
        public GazeToDisplayCoordinateMappingRecord(Gaze gaze, HeadPose headPose, Point2D display) {
            Gaze = gaze;
            HeadPose = headPose;
            Display = display;
        }

        public GazeToDisplayCoordinateMappingRecord(HeadPoseAndGaze headPoseAndGaze, Point2D display) : this(headPoseAndGaze.Gaze, headPoseAndGaze.HeadPose, display) {}

        [JsonIgnore]
        public HeadPoseAndGaze HeadPoseAndGaze => new HeadPoseAndGaze(HeadPose, Gaze);

        public bool Equals(GazeToDisplayCoordinateMappingRecord other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Display.Equals(other.Display);
        }
    }
}
