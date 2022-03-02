using System;
using System.Text.Json.Serialization;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class PoseAndGaze : IEquatable<PoseAndGaze> {

        [JsonInclude]
        public readonly Pose HeadPose;

        [JsonInclude]
        public readonly Gaze Gaze;

        [JsonInclude]
        public readonly Face Face;

        [JsonConstructor]
        public PoseAndGaze(Pose headPose, Gaze gaze, Face face) {
            HeadPose = headPose;
            Gaze = gaze;
            Face = face;
        }

        #region IEquatable
        public bool Equals(PoseAndGaze other) =>
            HeadPose.Equals(other.HeadPose)
            && Gaze.Equals(other.Gaze)
            && Face.Equals(other.Face)
            ;

        public override bool Equals(object obj) => obj is PoseAndGaze other ? Equals(obj) : false;

        public override int GetHashCode() => HashCode.Combine(
            HeadPose,
            Gaze,
            Face
        );

        public static bool operator ==(PoseAndGaze a, PoseAndGaze b) => a.Equals(b);

        public static bool operator !=(PoseAndGaze a, PoseAndGaze b) => !(a == b);
        #endregion
    }
}
