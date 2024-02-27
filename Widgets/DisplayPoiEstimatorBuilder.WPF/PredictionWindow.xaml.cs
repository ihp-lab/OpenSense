using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.Components.Psi.Imaging;
using OpenSense.Components.OpenFace;
using System.Threading.Tasks;
using System.Reflection;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    public sealed partial class PredictionWindow : Window {

        private static readonly MethodInfo AbandonAndDisposeMethod = typeof(Pipeline).GetMethod(nameof(Pipeline.Dispose), BindingFlags.NonPublic | BindingFlags.Instance);

        public PredictionWindow() {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }

        public IPoiOnDisplayEstimator Estimator;

        public bool FlipX { get; set; }

        public bool FlipY { get; set; }

        public string WebcamSymbolicLink { get; set; } = string.Empty;

        public int WebcamWidth { get; set; } = 1920;

        public int WebcamHeight { get; set; } = 1080;

        public int WebcamFrameRateNumerator { get; set; } = 30;

        public int WebcamFrameRateDenominator { get; set; } = 1;

        public float WebcamFx { get; set; } = 500;

        public float WebcamFy { get; set; } = 500;

        public float WebcamCx { get; set; } = 1920 / 2f;

        public float WebcamCy { get; set; } = 1080 / 2f;

        public bool NegateFlipX { get; set; } = true;

        private Pipeline pipeline;

        private void PredictionWindow_Input(object sender, EventArgs e) {
            if (DialogResult is not null) {
                return;
            }
            switch (e) {
                case KeyEventArgs key:
                    switch (key.Key) {
                        case Key.Escape:
                        case Key.Enter:
                            StopAndClose();
                            break;
                    }
                    break;
                case MouseButtonEventArgs mouse:
                    StopAndClose();
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            InitializePipeline();
        }

        private async void StopAndClose() {
            DialogResult = true;

            if (pipeline is not null) {
                Cursor = Cursors.Wait;
                await Task.Factory.StartNew(() => {
                    var action = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), pipeline, AbandonAndDisposeMethod);
                    action(true);
                });
            }

            Close();
        }

        private void InitializePipeline() {
            pipeline = Pipeline.Create(deliveryPolicy: DeliveryPolicy.LatestMessage);
            var webcamConfig = MediaCaptureConfiguration.Default;
            webcamConfig.DeviceId = WebcamSymbolicLink;
            webcamConfig.Width = WebcamWidth;
            webcamConfig.Height = WebcamHeight;
            webcamConfig.Framerate = WebcamFrameRateNumerator / WebcamFrameRateDenominator;
            webcamConfig.UseInSharedMode = true;
            var source = new MediaCapture(pipeline, webcamConfig);
            var flip = new FlipImageOperator(pipeline) { Horizontal = FlipX, Vertical = FlipY };
            source.PipeTo(flip.In);
            var openface = new OpenFace(pipeline) { FocalLengthX = WebcamFx, FocalLengthY = WebcamFy, CenterX = WebcamCx, CenterY = WebcamCy };
            flip.PipeTo(openface.In);
            var dispFlip = new FlipImageOperator(pipeline) { 
                Horizontal = FlipX ^ NegateFlipX, 
                Vertical = FlipY 
            };
            source.PipeTo(dispFlip.In);
            var joinedVideoFrame = openface.Out.Join(dispFlip.Out);
            joinedVideoFrame.Do(UpdateDisplay);
            pipeline.RunAsync();
        }

        private void UpdateDisplay((PoseAndEyeAndFace, Shared<Microsoft.Psi.Imaging.Image>) dataPoint, Envelope e) {
            var coordinate = Estimator.Predict(dataPoint.Item1);
            try {
                Dispatcher.Invoke(() => {
                    var point = new Point(coordinate.X * PredictionCanvas.ActualWidth, coordinate.Y * PredictionCanvas.ActualHeight);
                    EllipseGeometryPredictionCircle.Center = point;
                    var bitmap = dataPoint.Item2.Resource.ToBitmap();
                    using var memory = new MemoryStream();
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    ImageBrushVideoFrame.ImageSource = bitmapImage;
                }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(500));
            } catch (TimeoutException) {
            }
        }
    }
}
