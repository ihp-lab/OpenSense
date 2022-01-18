using System;
using System.Text.Json.Serialization;

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

        #region IEquatable
        public bool Equals(PoseAndGaze other) =>
            HeadPose.Equals(other.HeadPose)
            && Gaze.Equals(other.Gaze)
            ;

        public override bool Equals(object obj) => obj is PoseAndGaze other ? Equals(obj) : false;

        public override int GetHashCode() => HashCode.Combine(
            HeadPose,
            Gaze
        );

        public static bool operator ==(PoseAndGaze a, PoseAndGaze b) => a.Equals(b);

        public static bool operator !=(PoseAndGaze a, PoseAndGaze b) => !(a == b);
        #endregion
    }
}
