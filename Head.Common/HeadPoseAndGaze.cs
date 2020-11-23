using System;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class HeadPoseAndGaze : IEquatable<HeadPoseAndGaze> {

        public readonly HeadPose HeadPose;

        public readonly Gaze Gaze;

        [JsonConstructor]
        public HeadPoseAndGaze(HeadPose headPose, Gaze gaze) {
            HeadPose = headPose;
            Gaze = gaze;
        }

        public bool Equals(HeadPoseAndGaze other) {
            return Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose);
        }
    }
}
