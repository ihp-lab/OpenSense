// <copyright file="Calibration.xaml.cs" company="USC">
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Component.EyePointOfInterest.Common;
using OpenSense.Component.Imaging;
using OpenSense.Component.OpenFace;

namespace OpenSense.Wpf.Widget.DisplayPoiEstimatorBuilder {

    /// <summary>
    /// CalibrationWindow class.
    /// </summary>
    public partial class CaptureWindow : Window {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureWindow"/> class.
        /// </summary>
        public CaptureWindow() {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            Cursor = Cursors.Hand;
            DataContext = DataPoints;
        }

        public bool FlipX { get; set; }

        public bool FlipY { get; set; }

        public TimeSpan Duration { get; set; } = TimeSpan.Zero;

        public string WebcamSymbolicLink { get; set; } = string.Empty;

        public int WebcamWidth { get; set; } = 1920;

        public int WebcamHeight { get; set; } = 1080;

        public float WebcamFx { get; set; } = 500;

        public float WebcamFy { get; set; } = 500;

        public float WebcamCx { get; set; } = 1920 / 2f;

        public float WebcamCy { get; set; } = 1080 / 2f;

        public ObservableCollection<GazeToDisplayCoordinateMappingRecord> DataPoints { get; private set; } = new ObservableCollection<GazeToDisplayCoordinateMappingRecord>();

        private Storyboard pathAnimationStoryboard = new Storyboard();

        /// <summary>
        /// List of horizontal calibration lines.
        /// </summary>
        private List<Line> horizontalCalibrationLines = new List<Line>();

        /// <summary>
        /// List of vertical calibration lines.
        /// </summary>
        private List<Line> verticalCalibrationLines = new List<Line>();

        /// <summary>
        /// List of all calibration lines.
        /// </summary>
        private List<Line> allCalibrationLines = new List<Line>();

        /// <summary>
        /// Calibration grid loaded method.
        /// </summary>
        private void InitializeCanvas() {
            double location = 25d;
            double offset = 25d;
            double distance = 100d;
            bool parity = false;

            while (true) {
                var horizontalLine = new Line();
                var verticalLine = new Line();

                if (!parity) {
                    horizontalLine.X1 = offset;
                    horizontalLine.X2 = ActualWidth - offset;
                    horizontalLine.Y1 = location;
                    horizontalLine.Y2 = location;

                    horizontalLine.Stroke = Brushes.Black;
                    horizontalLine.StrokeThickness = 1;

                    verticalLine.X1 = ActualWidth - offset;
                    verticalLine.X2 = ActualWidth - offset;
                    verticalLine.Y1 = location;
                    verticalLine.Y2 = location + distance;

                    verticalLine.Stroke = Brushes.Black;
                    verticalLine.StrokeThickness = 1;
                } else {
                    horizontalLine.X1 = ActualWidth - offset;
                    horizontalLine.X2 = offset;
                    horizontalLine.Y1 = location;
                    horizontalLine.Y2 = location;

                    horizontalLine.Stroke = Brushes.Black;
                    horizontalLine.StrokeThickness = 1;

                    verticalLine.X1 = offset;
                    verticalLine.X2 = offset;
                    verticalLine.Y1 = location;
                    verticalLine.Y2 = location + distance;

                    verticalLine.Stroke = Brushes.Black;
                    verticalLine.StrokeThickness = 1;
                }

                if (location > ActualHeight) {
                    break;
                } else {
                    horizontalCalibrationLines.Add(horizontalLine);
                    CalibrationCanvas.Children.Add(horizontalLine);

                    allCalibrationLines.Add(horizontalLine);
                }

                if (location + distance > ActualHeight) {
                    break;
                } else {
                    verticalCalibrationLines.Add(verticalLine);
                    CalibrationCanvas.Children.Add(verticalLine);

                    allCalibrationLines.Add(verticalLine);
                }

                location += distance;
                parity = !parity;
            }
            EllipseGeometryCalibrationCircle.Center = new Point(horizontalCalibrationLines[0].X1, horizontalCalibrationLines[0].Y1);
            //EllipseGeometryRecordCircle.Center = EllipseGeometryCalibrationCircle.Center;
        }

        private bool CalibrationStarted = false;

        /// <summary>
        /// Calibration method.
        /// </summary>
        private void StartCalibration(object sender, EventArgs e) {
            if (CalibrationStarted) {
                return;
            }
            CalibrationStarted = true;

            pipeline.RunAsync();

            var segment = new PolyLineSegment();
            allCalibrationLines.ForEach(line => segment.Points.Add(new Point(line.X2, line.Y2)));
            var pathFigure = new PathFigure();
            pathFigure.StartPoint = new Point(allCalibrationLines.First().X1, allCalibrationLines.First().Y1);
            pathFigure.Segments.Add(segment);
            var animationPath = new PathGeometry();
            animationPath.Figures.Add(pathFigure);
            animationPath.Freeze();

            var centerPointAnimation = new PointAnimationUsingPath();
            centerPointAnimation.PathGeometry = animationPath;
            centerPointAnimation.Duration = Duration;

            Storyboard.SetTargetName(centerPointAnimation, nameof(EllipseGeometryCalibrationCircle));//Storyboard.SetTarget method not working
            Storyboard.SetTargetProperty(centerPointAnimation, new PropertyPath(EllipseGeometry.CenterProperty));

            pathAnimationStoryboard.Children.Add(centerPointAnimation);
            //pathAnimationStoryboard.Completed += CalibrationCompleted;
            pathAnimationStoryboard.AutoReverse = true;
            pathAnimationStoryboard.RepeatBehavior = RepeatBehavior.Forever;
            pathAnimationStoryboard.Begin(this);
            CompositionTarget.Rendering += CaptureCalibrationCircleCoordinate;
        }

        private bool windowClosed = false;

        private void CalibrationCompleted(object sender, EventArgs e) {
            if (windowClosed) {
                return;
            }
            DialogResult = true;
            Close();
        }

        private void CalibrationWindow_Key(object sender, EventArgs e) {
            void close() {
                DialogResult = true;
                Close();
            }
            switch (e) {
                case KeyEventArgs keyEvent:
                    if (CalibrationStarted) {
                        switch (keyEvent.Key) {
                            case Key.Escape:
                            case Key.Enter:
                                close();
                                break;
                        }
                    } else {
                        switch (keyEvent.Key) {
                            case Key.Escape:
                                close();
                                break;
                            case Key.Enter:
                                StartCalibration(sender, e);
                                break;
                        }
                    }
                    break;
                case MouseButtonEventArgs mouseEvent:
                    if (CalibrationStarted) {
                        close();
                    } else {
                        StartCalibration(sender, e);
                    }
                    break;
            }


        }

        #region pipeline

        private Pipeline pipeline;

        private DisplayCoordianteGenerator generator;

        private void InitializePipeline() {
            pipeline = Pipeline.Create();
            var webcamConfig = MediaCaptureConfiguration.Default;
            webcamConfig.DeviceId = WebcamSymbolicLink;
            webcamConfig.Width = WebcamWidth;
            webcamConfig.Height = WebcamHeight;
            webcamConfig.Framerate = 30;
            webcamConfig.UseInSharedMode = true;
            var source = new MediaCapture(pipeline, webcamConfig);
            var flip = new FlipColorVideo(pipeline) { FlipHorizontal = FlipX, FlipVertical = FlipY };
            source.PipeTo(flip.In, DeliveryPolicy.SynchronousOrThrottle);
            var openface = new OpenFace(pipeline) { CameraCalibFx = WebcamFx, CameraCalibFy = WebcamFy, CameraCalibCx = WebcamCx, CameraCalibCy = WebcamCy };
            flip.PipeTo(openface.In, DeliveryPolicy.SynchronousOrThrottle);
            generator = new DisplayCoordianteGenerator(pipeline);
            var record = openface.Out.Join(generator.Out, Reproducible.Nearest<Vector2>(), (gp, display) => new GazeToDisplayCoordinateMappingRecord(gp, display), DeliveryPolicy.SynchronousOrThrottle, DeliveryPolicy.SynchronousOrThrottle);
            record.Do((d, e) => {
                AddRecord(d, e);
            }, DeliveryPolicy.SynchronousOrThrottle);
        }

        private class DisplayCoordianteGenerator : IProducer<Vector2> {

            public Emitter<Vector2> Out { get; private set; }

            private DateTime lastTime;

            public DisplayCoordianteGenerator(Pipeline pipeline) {
                Out = pipeline.CreateEmitter<Vector2>(this, nameof(Out));
            }

            public void Post(EllipseGeometry ellipse, FrameworkElement frameworkElement) {
                var now = DateTime.UtcNow;
                if (now - lastTime <= TimeSpan.Zero) {
                    return;
                }
                lastTime = now;
                var raw = ellipse.Center;
                var relativeX = (float)(raw.X / frameworkElement.ActualWidth);
                var relativeY = (float)(raw.Y / frameworkElement.ActualHeight);
                var relative = new Vector2(relativeX, relativeY);
                Out.Post(relative, now);
            }
        }

        private void CaptureCalibrationCircleCoordinate(object sender, EventArgs e) {
            generator.Post(EllipseGeometryCalibrationCircle, CalibrationCanvas);
        }
        private void AddRecord(GazeToDisplayCoordinateMappingRecord record, Envelope envelope) {
            record = record.DeepClone();
            DataPoints.Add(record);
            Dispatcher.InvokeAsync(() => EllipseGeometryRecordCircle.Center = new Point(record.Display.X * CalibrationCanvas.ActualWidth, record.Display.Y * CalibrationCanvas.ActualHeight));//Do not do it in synchronize fasion, may dead lock with Window_Closing
        }

        private void UpdateDisplay(Shared<Microsoft.Psi.Imaging.Image> image, Envelope e) {
            image = image.DeepClone();
            try {
                Dispatcher.Invoke(() => {
                    var bitmap = image.Resource.ToBitmap();
                    using (var memory = new MemoryStream()) {
                        bitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        ImageBrushVideoFrame.ImageSource = bitmapImage;
                    }
                }, DispatcherPriority.Normal, CancellationToken.None, TimeSpan.FromMilliseconds(100));
            } catch (TimeoutException) {
            }
        }

        #endregion

        private void CalibrationWindow_Closing(object sender, EventArgs e) {
            CompositionTarget.Rendering -= CaptureCalibrationCircleCoordinate;
            pipeline?.Dispose();
        }

        private void CalibrationWindow_Loaded(object sender, RoutedEventArgs e) {
            InitializeCanvas();
            InitializePipeline();
        }
    }
}
