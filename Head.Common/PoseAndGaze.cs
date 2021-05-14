using System;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class PoseAndGaze : IEquatable<PoseAndGaze> {

        public readonly Pose HeadPose;

        public readonly Gaze Gaze;

        [JsonConstructor]
        public PoseAndGaze(Pose headPose, Gaze gaze) {
            HeadPose = headPose;
            Gaze = gaze;
        }

        public bool Equals(PoseAndGaze other) => Gaze.Equals(other.Gaze) && HeadPose.Equals(other.HeadPose);
    }
}
