using System;
using System.Windows;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using OpenSense.DataStructure;

namespace OpenSense.GazeToDisplayConverter {
    
    [Serializable]
    public class Record : IEquatable<Record> {
 
        public readonly Gaze Gaze;

        public readonly HeadPose HeadPose;

        /// <summary>
        /// 0 - 1 relative coordinate
        /// </summary>
        public readonly Point2D Display;

        [JsonConstructor]
        public Record(Gaze gaze, HeadPose headPose, Point2D display) {
            Gaze = gaze;
            HeadPose = headPose;
            Display = display;
        }

        public Record(HeadPoseAndGaze headPoseAndGaze, Point2D display) : this(headPoseAndGaze.Gaze, headPoseAndGaze.HeadPose, display) {}

        [JsonIgnore]
        public HeadPoseAndGaze HeadPoseAndGaze => new HeadPoseAndGaze(HeadPose, Gaze);

        public bool Equals(Record other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose) && Display.Equals(other.Display);
        }
    }
}
