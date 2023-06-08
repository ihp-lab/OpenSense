using Mediapipe.Net.Framework.Protobuf;

namespace OpenSense.Components.LibreFace {
    internal record struct Float2(float I0, float I1) {

        public static implicit operator Float2((float, float) p) => new Float2(p.Item1, p.Item2);
        public static implicit operator (float, float)(Float2 p) => (p.I0, p.I1);
        public static implicit operator Float2(NormalizedLandmark lm) => new Float2(lm.X, lm.Y);

        public static Float2 operator +(Float2 left, float right) => new Float2(left.I0 + right, left.I1 + right);
        public static Float2 operator +(Float2 left, Float2 right) => new Float2(left.I0 + right.I0, left.I1 + right.I1);

        public static Float2 operator -(Float2 left, Float2 right) => new Float2(left.I0 - right.I0, left.I1 - right.I1);

        public static Float2 operator *(Float2 left, float right) => new Float2(left.I0 * right, left.I1 * right);
        public static Float2 operator *(Float2 left, Float2 right) => new Float2(left.I0 * right.I0, left.I1 * right.I1);

        public static Float2 operator /(Float2 left, float right) => new Float2(left.I0 / right, left.I1 / right);

    }
}
