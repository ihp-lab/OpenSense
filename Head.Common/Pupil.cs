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

        public bool Equals(Pupil other) => Left.Equals(other.Left) && Right.Equals(other.Right);
    }
}
