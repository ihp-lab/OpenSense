namespace OpenSense.Components.PortableFACS {
    internal record struct Int3(int I0, int I1, int I2) {
        public static implicit operator Int3((int, int, int) p) => new Int3(p.Item1, p.Item2, p.Item3);
        public static implicit operator (int, int, int)(Int3 p) => (p.I0, p.I1, p.I2);
    }
}
