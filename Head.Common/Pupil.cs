using System;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;

namespace OpenSense.Component.Head.Common {
    [Serializable]
    public class Pupil : IEquatable<Pupil> {

        public readonly Point3D Left;

        public readonly Point3D Right;

        [JsonConstructor]
        public Pupil(Point3D left, Point3D right) {
            Left = left;
            Right = right;
        }

        public bool Equals(Pupil other) {
            return Left.Equals(other.Left) && Right.Equals(other.Right);
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
