namespace OpenSense.Wpf.Widget.DisplayPoiEstimatorBuilder {
    internal struct Resolution {

        public int Width;

        public int Height;

        public Resolution(int width, int height) {
            Width = width;
            Height = height;
        }

        public override string ToString() => $"{Width} × {Height}";


    }


}
