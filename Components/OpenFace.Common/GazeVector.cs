using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSense.Components.OpenFace {
    [Serializable]
    public sealed class GazeVector : IEquatable<GazeVector> {

        public Vector3 Left { get; }

        public Vector3 Right { get; }

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
