using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Psi;
using Microsoft.Psi.Media;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.Components.OpenFace;
using OpenSense.Components.Psi.Imaging;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {

    public sealed partial class CaptureWindow : Window {

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

        public int WebcamFrameRateNumerator { get; set; } = 30;

        public int WebcamFrameRateDenominator { get; set; } = 1;

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
        }

        //private bool windowClosed = false;

        //private void CalibrationCompleted(object sender, EventArgs e) {
        //    if (windowClosed) {
        //        return;
        //    }
        //    DialogResult = true;
        //    Close();
        //}

        private void CalibrationWindow_Key(object sender, EventArgs e) {
            if (DialogResult is not null) {
                return;
            }
            switch (e) {
                case KeyEventArgs keyEvent:
                    if (CalibrationStarted) {
                        switch (keyEvent.Key) {
                            case Key.Escape:
                            case Key.Enter:
                                StopAndClose();
                                break;
                        }
                    } else {
                        switch (keyEvent.Key) {
                            case Key.Escape:
                                StopAndClose();
                                break;
                            case Key.Enter:
                                StartCalibration(sender, e);
                                break;
                        }
                    }
                    break;
                case MouseButtonEventArgs mouseEvent:
                    if (CalibrationStarted) {
                        StopAndClose();
                    } else {
                        StartCalibration(sender, e);
                    }
                    break;
            }
        }

        private async void StopAndClose() {
            DialogResult = true;
            
            if (pipeline is not null) {
                pathAnimationStoryboard.Stop(this);
                Cursor = Cursors.Wait;
                await Task.Factory.StartNew(pipeline.Dispose);
            }

            Close();
        }

        #region pipeline

        private Pipeline pipeline;

        private DisplayCoordianteGenerator generator;

        private void InitializePipeline() {
            pipeline = Pipeline.Create(deliveryPolicy: DeliveryPolicy.LatestMessage);
            var frameRate = WebcamFrameRateNumerator / WebcamFrameRateDenominator;
            var webcamConfig = MediaCaptureConfiguration.Default;
            webcamConfig.DeviceId = WebcamSymbolicLink;
            webcamConfig.Width = WebcamWidth;
            webcamConfig.Height = WebcamHeight;
            webcamConfig.Framerate = frameRate;
            webcamConfig.UseInSharedMode = true;
            var source = new MediaCapture(pipeline, webcamConfig);
            var flip = new FlipImageOperator(pipeline) { Horizontal = FlipX, Vertical = FlipY };
            source.PipeTo(flip.In);
            var openface = new OpenFace(pipeline) { FocalLengthX = WebcamFx, FocalLengthY = WebcamFy, CenterX = WebcamCx, CenterY = WebcamCy };
            flip.PipeTo(openface.In);
            generator = new DisplayCoordianteGenerator(pipeline, EllipseGeometryCalibrationCircle, CalibrationCanvas, Dispatcher, frameRate);
            var record = openface.Out.Join(generator.Out, Reproducible.Nearest<Vector2>(), (gp, display) => new GazeToDisplayCoordinateMappingRecord(gp, display));
            record.Do(AddRecord);
        }

        private void AddRecord(GazeToDisplayCoordinateMappingRecord record, Envelope envelope) {
            record = record.DeepClone();
            DataPoints.Add(record);
            Dispatcher.InvokeAsync(() => EllipseGeometryRecordCircle.Center = new Point(record.Display.X * CalibrationCanvas.ActualWidth, record.Display.Y * CalibrationCanvas.ActualHeight));//Do not do it in synchronize fasion, may dead lock with Window_Closing
        }

        #endregion

        private void CalibrationWindow_Loaded(object sender, RoutedEventArgs e) {
            InitializeCanvas();
            InitializePipeline();
        }
    }
}
