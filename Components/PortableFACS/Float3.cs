using System;
using System.ComponentModel.DataAnnotations;

namespace OpenSense.Components.PortableFACS {
    internal record struct Float3(float I0, float I1, float I2) {

        public const int Length = 3;

        public float this[int index] => index switch {
            0 => I0,
            1 => I1,
            2 => I2,
            _ => throw new IndexOutOfRangeException(),
        };

        public static implicit operator Float3((float, float, float) p) => new Float3(p.Item1, p.Item2, p.Item3);
        public static implicit operator (float, float, float)(Float3 p) => (p.I0, p.I1, p.I2);
        public static explicit operator Float3((int, int, int, int) p) => new Float3(p.Item1, p.Item2, p.Item3);

        public static Float3 operator +(Float3 left, float right) => new Float3(left.I0 + right, left.I1 + right, left.I2 + right);
        public static Float3 operator +(Float3 left, Float3 right) => new Float3(left.I0 + right.I0, left.I1 + right.I1, left.I2 + right.I2);

        public static Float3 operator -(Float3 left, Float3 right) => new Float3(left.I0 - right.I0, left.I1 - right.I1, left.I2 - right.I2);

        public static Float3 operator *(Float3 left, float right) => new Float3(left.I0 * right, left.I1 * right, left.I2 * right);
        public static Float3 operator *(Float3 left, Float3 right) => new Float3(left.I0 * right.I0, left.I1 * right.I1, left.I2 * right.I2);

        public static Float3 operator /(Float3 left, float right) => new Float3(left.I0 / right, left.I1 / right, left.I2 / right);
    }
}
