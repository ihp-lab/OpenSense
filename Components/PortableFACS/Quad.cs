using System;

namespace OpenSense.Components.LibreFace {
    internal record struct Quad(Float2 I0, Float2 I1, Float2 I2, Float2 I3) {

        public const int Count = 4;

        public Float2 this[int index] {
            set {
                switch (index) {
                    case 0:
                        I0 = value;
                        break;
                    case 1:
                        I1 = value;
                        break;
                    case 2:
                        I2 = value;
                        break;
                    case 3:
                        I3 = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            get => index switch {
                0 => I0,
                1 => I1,
                2 => I2,
                3 => I3,
                _ => throw new IndexOutOfRangeException(),
            };
        }

        public Float4 this[string range] {
            get {
                switch (range) {
                    case ":,0":
                        return new Float4(I0.I0, I1.I0, I2.I0, I3.I0);
                    case ":,1":
                        return new Float4(I0.I1, I1.I1, I2.I1, I3.I1);
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}
