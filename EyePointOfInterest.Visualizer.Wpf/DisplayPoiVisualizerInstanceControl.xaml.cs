using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OpenSense.Component.EyePointOfInterest.Visualizer;

namespace OpenSense.Wpf.Component.EyePointOfInterest.Visualizer {
    public partial class DisplayPoiVisualizerInstanceControl : UserControl {
        public DisplayPoiVisualizerInstanceControl() {
            InitializeComponent();
        }

        private DisplayPoiVisualizer Visualizer => DataContext as DisplayPoiVisualizer;

        private static int GCD(int a, int b) {
            return b == 0 ? a : GCD(b, a % b);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            PresentationSource source = PresentationSource.FromVisual(this);
            var dpiX = 96.0;
            var dpiY = 96.0;
            if (source != null) {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            var width = (int)SystemParameters.PrimaryScreenWidth;
            var height = (int)SystemParameters.PrimaryScreenHeight;
            var div = GCD(width, height);
            width /= div;
            height /= div;
            var stride = width / 8;
            var pixels = new byte[height * stride];
            var myPalette = new BitmapPalette(new[] { Colors.LightGray});
            BackgroundImageBrush.ImageSource = BitmapSource.Create(width, height, dpiX, dpiY, PixelFormats.Indexed1, myPalette, pixels, stride);

            Visualizer.PropertyChanged += MoveCircle;
        }

        private void MoveCircle(object sender, PropertyChangedEventArgs args) {
            try {
                Dispatcher.Invoke(() => {
                    if (Visualizer != null && Visualizer.X is float x && Visualizer.Y is float y) {
                        var ratio = Math.Min(Canvas.RenderSize.Width / BackgroundImageBrush.ImageSource.Width, Canvas.RenderSize.Height / BackgroundImageBrush.ImageSource.Height);
                        var imageBrushWidth = BackgroundImageBrush.ImageSource.Width * ratio;
                        var imageBrushHeight = BackgroundImageBrush.ImageSource.Height * ratio;
                        var halfHoriPadding = (Canvas.ActualWidth - imageBrushWidth) / 2;
                        var halfVertPadding = (Canvas.ActualHeight - imageBrushHeight) / 2;
                        Circle.Center = new Point(halfHoriPadding + x * imageBrushWidth, halfVertPadding + y * imageBrushHeight);
                    } else {
                        Circle.Center = new Point(double.NegativeInfinity, double.NegativeInfinity);
                    }
                }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            } catch (TimeoutException) {

            }
        }
    }
}
