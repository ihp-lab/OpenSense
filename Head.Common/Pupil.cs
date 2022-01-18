using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class Pupil : IEquatable<Pupil> {

        public readonly Vector3 Left;

        public readonly Vector3 Right;

        [JsonConstructor]
        public Pupil(Vector3 left, Vector3 right) {
            Left = left;
            Right = right;
        }

        #region IEquatable
        public bool Equals(Pupil other) =>
            Left.Equals(other.Left)
            && Right.Equals(other.Right)
            ;

        public override bool Equals(object obj) => obj is Pupil other ? Equals(obj) : false;

        public override int GetHashCode() => HashCode.Combine(
            Left,
            Right
        );

        public static bool operator ==(Pupil a, Pupil b) => a.Equals(b);

        public static bool operator !=(Pupil a, Pupil b) => !(a == b);
        #endregion
    }
}
