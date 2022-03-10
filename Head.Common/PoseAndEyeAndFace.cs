using System;
using System.Text.Json.Serialization;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class PoseAndEyeAndFace : IEquatable<PoseAndEyeAndFace> {

        [JsonInclude]
        public readonly Pose Pose;

        [JsonInclude]
        public readonly Eye Eye;

        [JsonInclude]
        public readonly Face Face;

        [JsonConstructor]
        public PoseAndEyeAndFace(Pose headPose, Eye gaze, Face face) {
            Pose = headPose;
            Eye = gaze;
            Face = face;
        }

        #region IEquatable
        public bool Equals(PoseAndEyeAndFace other) =>
            Pose.Equals(other.Pose)
            && Eye.Equals(other.Eye)
            && Face.Equals(other.Face)
            ;

        public override bool Equals(object obj) => obj is PoseAndEyeAndFace other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            Pose,
            Eye,
            Face
        );

        public static bool operator ==(PoseAndEyeAndFace a, PoseAndEyeAndFace b) => a.Equals(b);

        public static bool operator !=(PoseAndEyeAndFace a, PoseAndEyeAndFace b) => !(a == b);
        #endregion
    }
}
