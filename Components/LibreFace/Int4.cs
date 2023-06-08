namespace OpenSense.Components.LibreFace {
    internal record struct Int4(int I0, int I1, int I2, int I3) {
        public static implicit operator Int4((int, int, int, int) p) => new Int4(p.Item1, p.Item2, p.Item3, p.Item4);
        public static implicit operator (int, int, int, int)(Int4 p) => (p.I0, p.I1, p.I2, p.I3);
    }
}
