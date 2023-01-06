using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Component.OpenFace.Common {
    [Serializable]
    public class GazeVector : IEquatable<GazeVector> {

        [JsonInclude]
        public readonly Vector3 Left;

        [JsonInclude]
        public readonly Vector3 Right;

        [JsonConstructor]
        public GazeVector(Vector3 left, Vector3 right) {
            Left = left;
            Right = right;
        }

        #region IEquatable
        public bool Equals(GazeVector other) =>
            Left.Equals(other.Left)
            && Right.Equals(other.Right)
            ;

        public override bool Equals(object obj) => obj is GazeVector other ? Equals(other) : false;

        public override int GetHashCode() => HashCode.Combine(
            Left,
            Right
        );

        public static bool operator ==(GazeVector a, GazeVector b) => a.Equals(b);

        public static bool operator !=(GazeVector a, GazeVector b) => !(a == b);
        #endregion
    }
}
