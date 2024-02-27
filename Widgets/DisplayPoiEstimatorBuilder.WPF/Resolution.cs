namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    internal readonly record struct Resolution {

        public int Width { get; }

        public int Height { get; }

        public int FrameRateNumerator { get; }

        public int FrameRateDenominator { get; }

        public Resolution(int width, int height, int frameRateNumerator, int frameRateDenominator) {
            Width = width;
            Height = height;
            FrameRateNumerator = frameRateNumerator;
            FrameRateDenominator = frameRateDenominator;
        }

        public override string ToString() => $"{Width} × {Height} @ {FrameRateNumerator / FrameRateDenominator:0.##}";
    }


}
