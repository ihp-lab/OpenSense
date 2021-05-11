using System;
using System.Numerics;
using Newtonsoft.Json;

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

        public bool Equals(Pupil other) {
            return Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Left.GetHashCode(), Right.GetHashCode());
        }

        public override bool Equals(object obj) {
            switch (obj) {
                case Pupil c:
                    return Equals(c);
                default:
                    return false;
            }
        }
    }
}
